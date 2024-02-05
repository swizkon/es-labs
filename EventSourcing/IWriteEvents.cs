namespace EventSourcing;

public interface IWriteEvents
{
    Task<WriteEventResult> WriteEventAsync(string streamName, object data)
        => WriteEventsAsync(streamName, null, new[]{data});

    Task<WriteEventResult> WriteEventsAsync(string streamName, IEnumerable<object> data)
        => WriteEventsAsync(streamName, null, data);

    Task<WriteEventResult> WriteEventsAsync(string streamName, long? expectedRevision, IEnumerable<object> data);
}