namespace EventSourcing;

public interface IEventTypeResolver
{
    Type? ResolveType(IDictionary<string, string> metadata);
}