using EventStore.Client;
using Newtonsoft.Json;
using System.Text;

namespace ES.Labs.Domain;

public static class EventStoreUtil
{
    public static EventStoreClient GetDefaultClient()
    {
        var settings = EventStoreClientSettings
            .Create("esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false");
        return new EventStoreClient(settings);
    }

    public static TEvent GetRecordedEventAs<TEvent>(EventRecord evt)
        => (TEvent) GetRecordedEvent(evt, typeof(TEvent));

    public static object GetRecordedEvent(EventRecord evt, Type type)
    {
        var data = Encoding.UTF8.GetString(evt.Data.Span);
        return JsonConvert.DeserializeObject(data, type);
    }
}