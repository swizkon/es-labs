using System.Reflection;

namespace EventSourcing.EventStoreDB;

public class DefaultEventResolver : IEventTypeResolver
{
    public Type? ResolveType(IDictionary<string, string> metadata)
    {
        var typeName = metadata!["CtrlType"];
        var type = Type.GetType(typeName);
        if (type != null)
        {
            Console.WriteLine($"DefaultEventResolver: {type}");
            return type;
        }

        var typeFromCallingAssembly = Assembly.GetCallingAssembly().GetType(typeName);
        if (typeFromCallingAssembly != null)
        {
            Console.WriteLine($"typeFromCallingAssembly: {typeFromCallingAssembly.FullName}");
            return typeFromCallingAssembly;
        }

        Console.WriteLine($"DefaultEventResolver failed to get type: {typeName}");
        return null;
    }
}