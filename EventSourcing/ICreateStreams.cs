namespace EventSourcing;

public interface ICreateStreams
{
    Task<WriteEventResult> CreateStreamAsync(string streamName, object data)
        => CreateStreamAsync(streamName, new[] { data });

    Task<WriteEventResult> CreateStreamAsync(string streamName, IEnumerable<object> data);
}