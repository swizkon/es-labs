namespace EventSourcing;

public record DomainEvent(
    string EventType,
    object EventData,
    ulong Revision);