using EventSourcing;
using EventSourcing.EventStoreDB;
using RetailRhythmRadar.Domain.Events;
using System.Reflection;

namespace RetailRhythmRadar.Domain.Projections;

public class CustomEventResolver(IEventTypeResolver defaultEventResolver) : IEventTypeResolver
{
    private readonly IEventTypeResolver _fallbackEventTypeResolver = new GreedyEventResolver(Assembly.GetExecutingAssembly());

    public Type? ResolveType(IDictionary<string, string> metadata)
    {
        var type = Type.GetType(metadata["ClrType"]);
        if (type != null)
        {
            // Console.WriteLine($"CustomEventResolver: {type}");
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

    private Type? ResolveTypeFromMetadata(IDictionary<string, string> metadata)
    {
        var type = metadata!["ClrType"];

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

        if (type.EndsWith(".ZoneManuallyAdjustedEvent"))
            return typeof(ZoneManuallyAdjustedEvent);

        return _fallbackEventTypeResolver.ResolveType(metadata);
    }
}