using System.Text;
using System.Text.Json;
using EventStore.Client;

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

    public static DomainEvent ResolveEvent(ResolvedEvent evt, IEventTypeResolver eventResolver)
    {
        var metadata = JsonSerializer.Deserialize<IDictionary<string, string>>(
                           Encoding.UTF8.GetString(evt.Event.Metadata.ToArray()))
                       ?? new Dictionary<string, string>();

        var eType = eventResolver.ResolveType(metadata);

        return new DomainEvent(
            EventType: eType?.FullName ?? string.Empty,
            EventData: GetRecordedEvent(evt.Event, eType!),
            Revision: evt.Event.EventNumber,
            Position: evt.OriginalPosition.GetValueOrDefault().CommitPosition);
    }
}
