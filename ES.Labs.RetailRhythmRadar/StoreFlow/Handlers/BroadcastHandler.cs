using ES.Labs.RetailRhythmRadar.Hubs;
using ES.Labs.RetailRhythmRadar.StoreFlow.Projections;
using EventSourcing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;

namespace ES.Labs.RetailRhythmRadar.StoreFlow.Handlers;

public class BroadcastHandler : IConsumer<StoreStateChanged>
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
            _logger.LogInformation("PushStoresState store {Store}, currentCount {currentCount} at {Date}", message.Store, currentCount, state.Date);
            
            await _hubContext.Clients.All.SendAsync("PushStoresState", message.Store, currentCount.ToString(), "50", context.CancellationToken);
        }

        foreach (var zone in message.Zones)
        {
            var zoneCount = state.ZoneVisitor.FirstOrDefault(x => x.Key == zone).Value;
            _logger.LogInformation("PushStoresState store {Store}, {zone}: {zoneCount} at {Date}", message.Store, zone, zoneCount, state.Date);

            await _hubContext.Clients.All.SendAsync("PushZoneState", message.Store, zone, zoneCount.ToString(), context.CancellationToken);
        }
    }
}