using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventSourcing.EventStoreDB;


public static class EventStoreDbUtils
{
    public static EventStoreClient GetDefaultClient(string connectionString)
    {
        var settings = EventStoreClientSettings.Create(connectionString);
        return new EventStoreClient(settings);
    }

    public static TEvent GetRecordedEventAs<TEvent>(EventRecord evt)
        => (TEvent)GetRecordedEvent(evt, typeof(TEvent));

    public static object GetRecordedEvent(EventRecord evt, Type type)
    {
        var data = Encoding.UTF8.GetString(evt.Data.Span);
        return JsonSerializer.Deserialize(data, type)!;
    }

    /*
     *_enrichMetaData = enrichMetaData;
       var connectionString = configuration.GetConnectionString("EVENTSTORE");
       
       if (string.IsNullOrWhiteSpace(connectionString))
       {
       throw new ArgumentException("EVENTSTORE connection string is missing", nameof(connectionString));
       }
       
       var settings = EventStoreClientSettings.Create(connectionString);
       logger.LogInformation("Setting up EventStoreClient");
       _client = new EventStoreClient(settings);
     */
}

    /*
public class EventDataBuilder
{
    private readonly EventStoreClient _client;

    private static readonly ConcurrentDictionary<string, (string, Type)> EventTypes = new();

    public EventDataBuilder(IConfiguration configuration, ILogger<EventDataBuilder> logger)
    {
        var connectionString = configuration.GetConnectionString("EVENTSTORE");

        var settings = EventStoreClientSettings.Create(connectionString);
        logger.LogInformation("Setting up EventStoreClient");
        _client = new EventStoreClient(settings);
    }
    public async Task WriteEventAsync(string streamName, params object[] data)
    {
        await _client.AppendToStreamAsync(
            streamName: streamName,
            expectedState: StreamState.Any,
            eventData: BuildEventData(data));
    }

    public async IAsyncEnumerable<DomainEvent> ReadEventsAsync(
        string streamName,
        StreamPosition? revision = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var events = _client.ReadStreamAsync(Direction.Forwards, streamName, revision?.Next() ?? StreamPosition.Start, cancellationToken: cancellationToken);
        var state = await events.ReadState;
        if (state == ReadState.StreamNotFound)
        {
            yield break;
        }

        await foreach (var @event in events)
        {
            yield return ResolveEvent(@event);
        }
    }

    private static DomainEvent ResolveEvent(ResolvedEvent evt)
    {
        var (eventTypeName, eventType) = EventTypes.GetOrAdd(evt.Event.EventType, (s) =>
        {
            var metadata = JsonSerializer.Deserialize<IDictionary<string, string>>(
                Encoding.UTF8.GetString(evt.Event.Metadata.ToArray()));

            var eType = Type.GetType(metadata!["CtrlType"])!;
            return (eType.FullName!, eType);
        });

        return new DomainEvent(
            EventType: eventTypeName,
            EventData: GetRecordedEvent(evt.Event, eventType),
            Revision: evt.Event.EventNumber);
    }

    private static IEnumerable<EventData> BuildEventData(params object[] data)
        => data.Select(BuildEventData);

    private static EventData BuildEventData(object data)
    {
        return new EventData(
            eventId: Uuid.NewUuid(),
            type: data.GetType().Name.ToLower(),
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data)),
            metadata: BuildMetadata(data)
        );
    }

    private static ReadOnlyMemory<byte> BuildMetadata(object data)
    {
        var metadata = new
        {
            Timestamp = DateTime.UtcNow.ToString("o"),
            CtrlType = data.GetType().FullName,
            data.GetType().AssemblyQualifiedName
        };
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(metadata));
    }

    private static object GetRecordedEvent(EventRecord evt, Type type)
    {
        var data = Encoding.UTF8.GetString(evt.Data.Span);
        return JsonSerializer.Deserialize(data, type)!;
    }

}
    */