using EventStore.Client;
using System.Text;
using System.Text.Json;

namespace ES.Labs.Domain;

public static class EventStoreUtil
{
    //public static EventStoreClient GetDefaultClient(string connectionString )
    //{
    //    var settings = EventStoreClientSettings.Create(connectionString);
    //    return new EventStoreClient(settings);
    //}

    //public static TEvent GetRecordedEventAs<TEvent>(EventRecord evt)
    //    => (TEvent) GetRecordedEvent(evt, typeof(TEvent));

    //public static object GetRecordedEvent(EventRecord evt, Type type)
    //{
    //    var data = Encoding.UTF8.GetString(evt.Data.Span);
    //    return JsonSerializer.Deserialize(data, type)!;
    //}
}