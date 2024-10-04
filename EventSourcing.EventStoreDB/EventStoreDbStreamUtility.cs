using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventSourcing.EventStoreDB;

public class EventStoreDbStreamUtility : IReadStreams, ICreateStreams, IWriteEvents
{
    private readonly IEnrichMetaData _enrichMetaData;
    private readonly EventStoreClient _client;

    private static readonly ConcurrentDictionary<string, (string, Type)> EventTypes = new();

    public EventStoreDbStreamUtility(
        IConfiguration configuration,
        IEnrichMetaData enrichMetaData,
        ILogger<EventStoreDbStreamUtility> logger)
    {
        _enrichMetaData = enrichMetaData;
        var connectionString = configuration.GetConnectionString("EVENTSTORE");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("EVENTSTORE connection string is missing", nameof(connectionString));
        }

        var settings = EventStoreClientSettings.Create(connectionString);
        logger.LogInformation("Setting up EventStoreClient");
        _client = new EventStoreClient(settings);
    }
    
    public async Task<WriteEventResult> CreateStreamAsync(string streamName, IEnumerable<object> data)
    {
        var result = await _client.AppendToStreamAsync(
            streamName: streamName,
            expectedState: StreamState.NoStream,
            eventData: BuildEventData(data, enrichMetaData: _enrichMetaData));

        return new WriteEventResult(StreamName: streamName,
            Revision: result.NextExpectedStreamRevision,
            Position: result.LogPosition.CommitPosition);
    }

    public async Task<WriteEventResult> WriteEventsAsync(string streamName, long? expectedRevision, IEnumerable<object> data)
    {
        var result = expectedRevision == null ?
            await _client.AppendToStreamAsync(
                streamName: streamName,
                expectedState: StreamState.Any,
                eventData: BuildEventData(data, enrichMetaData: _enrichMetaData))
            :
            await _client.AppendToStreamAsync(
                streamName: streamName,
                expectedRevision: StreamRevision.FromInt64(expectedRevision.GetValueOrDefault()),
                eventData: BuildEventData(data, enrichMetaData: _enrichMetaData));

        return new WriteEventResult(StreamName: streamName,
            Revision: result.NextExpectedStreamRevision,
            Position: result.LogPosition.CommitPosition);
    }

    private static IEnumerable<EventData> BuildEventData(IEnumerable<object> data, IEnrichMetaData enrichMetaData)
        => data.Select(x => BuildEventData(x, enrichMetaData));

    private static EventData BuildEventData(object data, IEnrichMetaData enrichMetaData)
    {
        var eventType = data.GetType().Name;
        return new EventData(
            eventId: Uuid.NewUuid(),
            type: eventType[..1].ToLowerInvariant() + eventType[1..],
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data)),
            metadata: BuildMetadata(data, enrichMetaData)
        );
    }

    private static ReadOnlyMemory<byte> BuildMetadata(object data, IEnrichMetaData enrichMetaData)
    {
        var metadata = new Dictionary<string, string>
        {
            {"Timestamp", DateTime.UtcNow.ToString("o")},
            {"ClrType", data.GetType().FullName!},
            {"AssemblyQualifiedName", data.GetType().AssemblyQualifiedName!}
        };

        var enrichedMetadata = enrichMetaData.Enrich(metadata);

        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(enrichedMetadata));
    }

    public async IAsyncEnumerable<DomainEvent> ReadEventsAsync(
        string streamName,
        ulong? revision = null,
        IEventTypeResolver? resolver = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        StreamPosition? streamRevision = revision.HasValue ? new StreamPosition(revision.Value) : null;
        var events = _client.ReadStreamAsync(Direction.Forwards, streamName, streamRevision?.Next() ?? StreamPosition.Start, resolveLinkTos: true, cancellationToken: cancellationToken);
        var state = await events.ReadState;
        if (state == ReadState.StreamNotFound)
        {
            yield break;
        }

        await foreach (var @event in events)
        {
            yield return ResolveEvent(@event, resolver ?? new DefaultEventResolver());
        }
    }

    private static DomainEvent ResolveEvent(ResolvedEvent evt, IEventTypeResolver eventResolver)
    {
        if(evt.Event == null)
        {
            var empty = new object();
            return new DomainEvent(
                EventType: empty.GetType().Name,
                EventData: empty,
                // Revision: evt.Event.EventNumber,
                Revision: evt.OriginalEventNumber,
                Position: evt.OriginalPosition.GetValueOrDefault().CommitPosition);
        }

        var (eventTypeName, eventType) = EventTypes.GetOrAdd(evt.Event.EventType, (_) =>
        {
            var metadata = JsonSerializer.Deserialize<IDictionary<string, string>>(
                Encoding.UTF8.GetString(evt.Event.Metadata.ToArray()))
                    ?? new Dictionary<string, string>();

            var eType = eventResolver.ResolveType(metadata);
            return eType is null
                ? throw new InvalidOperationException($"Could not resolve type for event {evt.Event.EventType} ({metadata["CtrlType"]})")
                : (eType.FullName!, eType);
        });

        return new DomainEvent(
            EventType: eventTypeName,
            EventData: GetRecordedEvent(evt.Event, eventType),
            // Revision: evt.Event.EventNumber,
            Revision: evt.OriginalEventNumber,
            Position: evt.OriginalPosition.GetValueOrDefault().CommitPosition);
    }

    private static object GetRecordedEvent(EventRecord evt, Type type)
    {
        var data = Encoding.UTF8.GetString(evt.Data.Span);
        return JsonSerializer.Deserialize(data, type)!;
    }
}