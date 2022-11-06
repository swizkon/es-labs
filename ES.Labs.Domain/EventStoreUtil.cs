using EventStore.Client;

namespace ES.Labs.Domain;

public static class EventStoreUtil
{
    public static EventStoreClient GetDefaultClient()
    {
        var settings = EventStoreClientSettings
            .Create("esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false");
        return new EventStoreClient(settings);
    }
}