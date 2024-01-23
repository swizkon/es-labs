namespace EventSourcing;

public record WriteEventResult(
    string StreamName,
    ulong Revision,
    ulong Position);