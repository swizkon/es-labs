using EventSourcing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using RetailRhythmRadar.Domain.Events;
using RetailRhythmRadar.Domain.Projections;
using RetailRhythmRadar.Hubs;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace RetailRhythmRadar.Domain.Handlers;

public class BroadcastHandler : IConsumer<StoreStateChanged>,
    IConsumer<ZoneManuallyClearedEvent>
{
    private readonly IReadStreams _eventStreams;
    private readonly IDistributedCache _cache;
    private readonly IHubContext<MessageExchangeHub> _hubContext;
    private readonly ILogger<BroadcastHandler> _logger;

#pragma warning disable IDE0290
    public BroadcastHandler(
        HttpClient httpClient,
        IReadStreams eventStreams,
        IDistributedCache cache,
        IHubContext<MessageExchangeHub> hubContext,
        ILogger<BroadcastHandler> logger)
#pragma warning restore IDE0290
    {
        // Keep this since the test setup does not seem to work incorrectly with primary constructors
        _cache = cache;
        _hubContext = hubContext;
        _logger = logger;
        _eventStreams = eventStreams;
    }

    public async Task Consume(ConsumeContext<StoreStateChanged> context)
    {
        var message = context.Message;
        var state = await new SingleStoreProjection(message.Store, message.Date)
            .WithCache(_cache)
            .WithEventDataBuilder(_eventStreams)
            .BuildAsync(context.CancellationToken);

        if (message.StoreChanged)
        {
            var currentCount = state.ZoneVisitor.Sum(x => x.Value);
            _logger.LogInformation("StoreStateChanged store {Store}, currentCount {currentCount} at {Date}", message.Store, currentCount, state.Date);

            /*await*/ _hubContext.Clients.Group("storestates").SendAsync("StoreStateChanged", message.Store, currentCount, 50, context.CancellationToken);
        }

        foreach (var zone in message.Zones)
        {
            var group = $"store-{message.Store}-states";
            var zoneCount = state.ZoneVisitor.FirstOrDefault(x => x.Key == zone).Value;
            _logger.LogInformation("ZoneStateChanged store:{Store} zone:{zone} {zoneCount} at {Date}", message.Store, zone, zoneCount, state.Date);

            /*await*/ _hubContext.Clients.Group(group).SendAsync("ZoneStateChanged", message.Store, zone, zoneCount, 50, context.CancellationToken);
        }
    }

    public async Task Consume(ConsumeContext<ZoneManuallyClearedEvent> context)
    {
        var message = context.Message;

        var group = $"store-{message.Store}-states";
        const int zoneCount = 0;

        var state = await new SingleStoreProjection(message.Store, message.Timestamp)
            .WithCache(_cache)
            .WithEventDataBuilder(_eventStreams)
            .BuildAsync(context.CancellationToken);

        _logger.LogInformation("ZoneManuallyClearedEvent store:{Store} zone:{zone} {zoneCount} at {Date}", message.Store, message.Zone, zoneCount, message.Timestamp);
        _hubContext.Clients.Group(group).SendAsync("ZoneStateChanged", message.Store, message.Zone, zoneCount, 50, context.CancellationToken);

        var storeCount = state.ZoneVisitor.Sum(x => x.Value);
        _hubContext.Clients.Group("storestates").SendAsync("StoreStateChanged", message.Store, storeCount, 50, context.CancellationToken);
    }
}