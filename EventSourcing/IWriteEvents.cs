namespace EventSourcing;

public interface IWriteEvents
{
    Task<WriteEventResult> WriteEventAsync(string streamName, params object[] data);
}