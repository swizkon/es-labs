using System.Reflection;

namespace EventSourcing.EventStoreDB;

public class GreedyEventResolver : IEventTypeResolver
{
    private Assembly? _assembly;

    public GreedyEventResolver(Assembly? assembly)
    {
        _assembly = assembly;
    }

    public Type? ResolveType(IDictionary<string, string> metadata)
    {
        var typeName = metadata["CtrlType"];

        var defaultAssembly = _assembly?.GetType(typeName);
        if (defaultAssembly != null)
        {
            Console.WriteLine($"{GetType().Name} defaultAssembly: {defaultAssembly.FullName}");
            return defaultAssembly;
        }

        var asm = Assembly.GetEntryAssembly();

        var typeFromCallingAssembly = asm?.GetType(typeName);
        if (typeFromCallingAssembly != null)
        {
            Console.WriteLine($"{GetType().Name} typeFromCallingAssembly: {typeFromCallingAssembly.FullName}");
            return typeFromCallingAssembly;
        }

        var searchTypeFromCallingAssembly = asm?
            .GetTypes()
            .FirstOrDefault(x =>
            {
                Console.WriteLine($"{typeName} can be {x.Name}");
                return typeName.EndsWith($".{x.Name}");
            });
        if (searchTypeFromCallingAssembly != null)
        {
            Console.WriteLine($"{GetType().Name} searchTypeFromCallingAssembly: {searchTypeFromCallingAssembly.FullName}");
            return searchTypeFromCallingAssembly;
        }

        Console.WriteLine($"{GetType().Name} failed to get type: {typeName}");
        return null;
    }
}