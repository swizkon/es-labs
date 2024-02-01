namespace EventSourcing;

public interface IWriteEvents
{
    Task<WriteEventResult> WriteEventAsync(string streamName, params object[] data);
    
    Task<WriteEventResult> WriteEventAsync(string streamName, long? expectedRevision, params object[] data);
}