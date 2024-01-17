namespace EventSourcing.EventStoreDB;

public class NoOpEventResolver : IEventTypeResolver
{
    public Type? ResolveType(IDictionary<string, string> metadata) => null;
}