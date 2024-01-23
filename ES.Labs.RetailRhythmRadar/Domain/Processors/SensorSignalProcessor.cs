using EventSourcing;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using RetailRhythmRadar.Domain.Events;
using RetailRhythmRadar.Domain.Handlers;

namespace RetailRhythmRadar.Domain.Processors;

public class SensorSignalProcessor : IProcess<TurnstilePassageDetected>
{
    private readonly IWriteEvents _eventWriter;
    private readonly IBus _bus;
    private readonly ILogger<SensorSignalProcessor> _logger;

#pragma warning disable IDE0290
    public SensorSignalProcessor(
        IWriteEvents eventWriter,
        IBus bus,
        ILogger<SensorSignalProcessor> logger)
#pragma warning restore IDE0290
    {
        // Keep this since the test setup does not seem to work correctly primary constructors
        _eventWriter = eventWriter;
        _bus = bus;
        _logger = logger;
    }

    public async Task<IEnumerable<WriteEventResult>> Handle(TurnstilePassageDetected message)
    {
        _logger.LogInformation("Processing message {message}", message);
        var results = new List<WriteEventResult>();
        var eventGroups = new List<(string, object)>();

        var turnstileStream = $"turnstile-{message.Turnstile.SerialNumber}";

        eventGroups.Add((turnstileStream, message));

        AppendStoreEvents(message, eventGroups);

        AppendZoneEvents(message, eventGroups);

        foreach (var groupedEvents in eventGroups.GroupBy(x => x.Item1))
        {
            var stream = groupedEvents.Key;
            var events = groupedEvents.ToList().Select(x => x.Item2).ToArray();

            var r = await _eventWriter.WriteEventAsync(stream, events);
            results.Add(r);
        }

        // Check for side effects
        var affectedZones = eventGroups
            .Select(x => x.Item2)
            .OfType<ZoneDomainEvent>()
            .Select(x => x.Zone)
            .Distinct()
            .ToArray();

        await _bus.Publish(new StoreStateChanged
        {
            Date = message.Timestamp.Date,
            Store = message.Turnstile.GetStore(),
            Zones = affectedZones,
            StoreChanged = eventGroups.Any(x => x.Item2 is StoreEnteredEvent or StoreExitedEvent)
        });

        return results;
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