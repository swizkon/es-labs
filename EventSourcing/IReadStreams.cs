namespace EventSourcing;

public interface IReadStreams
{
    IAsyncEnumerable<DomainEvent> ReadEventsAsync(
        string streamName,
        ulong? revision = null,
        IEventTypeResolver? resolver = null,
        CancellationToken cancellationToken = default);
}