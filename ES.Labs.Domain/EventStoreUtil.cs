﻿using EventStore.Client;
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

    public static TEvent GetRecordedEventAs<TEvent>(ResolvedEvent resolvedEvent)
    {
        var data = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
        return JsonConvert.DeserializeObject<TEvent>(data);
    }

    public static object? GetRecordedEvent(ResolvedEvent resolvedEvent, Type type)
    {
        var t = Type.GetType(resolvedEvent.Event.EventType);
        if (t == null) return null;

        var data = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
        return JsonConvert.DeserializeObject(data, type);
    }
}