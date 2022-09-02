//using System.Text;
//using Newtonsoft.Json;
//using EventStore.ClientAPI;

//public class EventStoreUtil
//{
//    readonly IEventStoreConnection tcpConnection;

//    public EventStoreUtil(IEventStoreConnection tcpConnection)
//        => this.tcpConnection = tcpConnection;

//    public static EventStore.Client.EventData ToJsonEventData(
//        object @event,
//        string eventType,
//        object? metadata = null
//    ) =>
//        new EventData(
//            EventStore.Client.Uuid.NewUuid(),
//            eventType,
//            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
//            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata ?? new { }))
//        );

//    public Task AppendEvents(
//        string streamName,
//        long version,
//        params object[] events
//    )
//    {
//        var preparedEvents = events
//            .Select(ToEventData)
//            .ToArray();

//        return tcpConnection.AppendToStreamAsync(
//            streamName,
//            version,
//            preparedEvents
//        );

//        static EventData ToEventData(object @event) =>
//            new EventData(
//                Guid.NewGuid(),
//                @event.GetType().Name.ToLowerInvariant(),
//                true,
//                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event))
//            );
//    }

//    public Task AppendEvents(
//        string streamName,
//        params object[] events
//    )
//        => AppendEvents(streamName, ExpectedVersion.Any, events);

//    public async Task<IEnumerable<object>> LoadEvents(string stream)
//    {
//        const int pageSize = 4096;

//        var start = 0;
//        var events = new List<object>();

//        do
//        {
//            var page = await tcpConnection.ReadStreamEventsForwardAsync(
//                stream, start, pageSize, true
//            );

//            if (page.Status == SliceReadStatus.StreamNotFound)
//                throw new ArgumentOutOfRangeException(
//                    nameof(stream), quot; Stream '{stream}' was not found"
//                );

//            events.AddRange(
//                page.Events.Select(Deserialize)
//            );
//            if (page.IsEndOfStream) break;

//            start += pageSize;
//        } while (true);

//        return events;

//        static object Deserialize(this ResolvedEvent resolvedEvent)
//        {
//            var dataType = TypeMapper.GetType(resolvedEvent.Event.EventType);
//            var jsonData = Encoding.UTF8.GetString(resolvedEvent.Event.Data);
//            var data = JsonConvert.DeserializeObject(jsonData, dataType);
//            return data;
//        }
//    }

//    public async Task<bool> StreamExists(string stream)
//    {
//        var result = await tcpConnection.ReadEventAsync(stream, 1, false);
//        return result.Status != EventReadStatus.NoStream;
//    }
//}