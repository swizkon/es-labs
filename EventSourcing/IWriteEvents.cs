namespace EventSourcing;

public interface IWriteEvents
{
    Task WriteEventAsync(string streamName, params object[] data);
}