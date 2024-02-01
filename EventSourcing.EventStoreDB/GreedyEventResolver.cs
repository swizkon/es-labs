using System.Reflection;

namespace EventSourcing.EventStoreDB;

public class GreedyEventResolver : IEventTypeResolver
{
    public Type? ResolveType(IDictionary<string, string> metadata)
    {
        var typeName = metadata["CtrlType"];

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