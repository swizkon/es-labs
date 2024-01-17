using EventSourcing;
using MassTransit;
using RetailRhythmRadar.Domain.Events;

namespace RetailRhythmRadar.Domain.Handlers;

public class SensorSignalsAdapter : IConsumer<TurnstilePassageDetected>
{
    private readonly IWriteEvents _eventWriter;
    private readonly ILogger<SensorSignalsAdapter> _logger;

#pragma warning disable IDE0290
    public SensorSignalsAdapter(IWriteEvents eventWriter, ILogger<SensorSignalsAdapter> logger)
#pragma warning restore IDE0290
    {
        // Keep this since the test setup does not seem to work correctly primary constructors
        _eventWriter = eventWriter;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TurnstilePassageDetected> context)
    {
        var eventGroups = new List<(string, object)>();
        var message = context.Message;

        var turnstileStream = $"turnstile-{message.Turnstile.SerialNumber}";

        eventGroups.Add((turnstileStream, message));

        AppendStoreEvents(message, eventGroups);

        AppendZoneEvents(message, eventGroups);

        foreach (var groupedEvents in eventGroups.GroupBy(x =>x.Item1))
        {
            var stream = groupedEvents.Key;
            var events = groupedEvents.ToList().Select(x => x.Item2).ToArray();

            await _eventWriter.WriteEventAsync(stream, events);
        }

        // Check for side effects
        var affectedZones = eventGroups
            .Select(x => x.Item2)
            .OfType<ZoneDomainEvent>()
            .Select(x => x.Zone)
            .Distinct()
            .ToArray();

        await context.Publish(new StoreStateChanged
        {
            Date = message.Timestamp.Date,
            Store = message.Turnstile.GetStore(),
            Zones = affectedZones,
            StoreChanged = eventGroups.Any(x => x.Item2 is StoreEnteredEvent or StoreExitedEvent)
        });
    }

    private void AppendZoneEvents(TurnstilePassageDetected message, List<(string, object)> eventGroups)
    {
        var store = message.Turnstile.GetStore();
        var storeStream = $"store-{store}-{message.Timestamp.Date:yyyy-MM-dd}";

        var (from, to) = GetLocations(message);
        
        // Leaving a zone...
        if (from != "0")
        {
            eventGroups.Add((storeStream, new ZoneExitedEvent
            {
                Store = store, 
                Zone = from, 
                Timestamp = message.Timestamp
            }));
        }

        // Entering zone
        if (to != "0")
        {
            eventGroups.Add((storeStream, new ZoneEnteredEvent
            {
                Store = store,
                Zone = to,
                Timestamp = message.Timestamp
            }));
        }
    }

    private static void AppendStoreEvents(TurnstilePassageDetected message, List<(string, object)> eventGroups)
    {
        var (from, to) = GetLocations(message);

        var allStoresStream = $"stores-{message.Timestamp.Date:yyyy-MM-dd}";
        var isEnterEvent = from == "0" && to != "0";

        if (isEnterEvent)
        {
            var enteredEvent = new StoreEnteredEvent { Store = message.Turnstile.GetStore(), Timestamp = message.Timestamp };
            eventGroups.Add((allStoresStream, enteredEvent));
        }

        var isExitEvent = from != "0" && to == "0";
        if (isExitEvent)
        {
            var storeExitedEvent = new StoreExitedEvent { Store = message.Turnstile.GetStore(), Timestamp = message.Timestamp };
            eventGroups.Add((allStoresStream, storeExitedEvent));
        }
    }

    private static (string, string) GetLocations(TurnstilePassageDetected message)
    {
        var location = message.Turnstile.GetLocation().PadLeft(2, '0')[..2];

        var from = message.Direction == TurnstileDirection.Clockwise
            ? location[..1]
            : location[1..];

        var to = message.Direction == TurnstileDirection.Clockwise
            ? location[1..]
            : location[..1];

        return (from, to);
    }
}