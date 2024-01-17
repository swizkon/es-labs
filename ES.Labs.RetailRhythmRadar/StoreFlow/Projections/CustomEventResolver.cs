using EventSourcing;
using RetailRhythmRadar.StoreFlow.Events;

namespace RetailRhythmRadar.StoreFlow.Projections;

public class CustomEventResolver(IEventTypeResolver defaultEventResolver) : IEventTypeResolver
{
    public Type? ResolveType(IDictionary<string, string> metadata)
    {
        var type = Type.GetType(metadata["CtrlType"]);
        if (type != null)
        {
            Console.WriteLine($"CustomEventResolver: {type}");
            return type;
        }

        var resolvedType = defaultEventResolver.ResolveType(metadata);
        if (resolvedType != null)
        {
            return resolvedType;
        }

        // Check specials...
        return ResolveTypeFromMetadata(metadata);
    }

    private static Type? ResolveTypeFromMetadata(IDictionary<string, string> metadata)
    {
        var type = metadata!["CtrlType"];

        Console.WriteLine($"ResolveTypeFromMetadata: {type}");
        
        if (type.EndsWith(".TurnstilePassageDetected"))
            return typeof(TurnstilePassageDetected);

        if (type.EndsWith(".ZoneEnteredEvent"))
            return typeof(ZoneEnteredEvent);

        if (type.EndsWith(".ZoneExitedEvent"))
            return typeof(ZoneExitedEvent);

        if (type.EndsWith(".StoreEnteredEvent"))
            return typeof(StoreEnteredEvent);

        if (type.EndsWith(".StoreExitedEvent"))
            return typeof(StoreExitedEvent);

        if (type.EndsWith(".ZoneManuallyClearedEvent"))
            return typeof(ZoneManuallyClearedEvent);

        return null;
    }
}