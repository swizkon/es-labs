using EventSourcing;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using RetailRhythmRadar.StoreFlow.Commands;
using RetailRhythmRadar.StoreFlow.Events;
using RetailRhythmRadar.StoreFlow.Projections;

namespace RetailRhythmRadar.StoreFlow.Handlers;

public class ZoneHandler :
        // IConsumer<EnteringZone>,
        // IConsumer<LeavingZone>,
        IConsumer<ResetZone>,
        IConsumer<ZoneManuallyClearedEvent>
{
    private readonly IWriteEvents _eventWriter;
    private readonly IReadStreams _streamReader;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ZoneHandler> _logger;

#pragma warning disable IDE0290
    public ZoneHandler(
        IWriteEvents eventWriter,
        IReadStreams streamReader,
        IDistributedCache cache,
        ILogger<ZoneHandler> logger)
#pragma warning restore IDE0290
    {
        // Keep this since the test setup does not seem to work correctly primary constructors
        _eventWriter = eventWriter;
        _streamReader = streamReader;
        _cache = cache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ResetZone> context)
    {
        // Simple rehydrate the state
        var state = await new SingleStoreProjection(context.Message.Store, context.Message.Timestamp)
            .WithCache(_cache)
            .WithEventDataBuilder(_streamReader)
            .BuildAsync(context.CancellationToken);
        
        // TODO Some validation etc...
        var zoneVisitors = state.ZoneVisitor.FirstOrDefault(z => z.Key == context.Message.Zone).Value;
        if (zoneVisitors == 0)
        {
            _logger.LogWarning("Zone {Zone} in store {Store} was already empty", context.Message.Zone, context.Message.Store);
            return;
        }

        var evt = new ZoneManuallyClearedEvent
        {
            Store = context.Message.Store,
            Zone = context.Message.Zone,
            VisitorsBeforeReset = zoneVisitors,
            Who = context.Message.Who,
            Reason = context.Message.Reason,
            Timestamp = context.Message.Timestamp
        };

        var storeStream = $"store-{context.Message.Store}-{context.Message.Timestamp.Date:yyyy-MM-dd}";
        await _eventWriter.WriteEventAsync(storeStream, evt);

        await context.Publish(evt, cancellationToken: context.CancellationToken);
    }

    /// <summary>
    /// Maybe this would belong in a StoreActor
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task Consume(ConsumeContext<ZoneManuallyClearedEvent> context)
    {
        var message = context.Message;

        // Simple rehydrate the state
        var state = await new SingleStoreProjection(message.Store, message.Timestamp)
            .WithCache(_cache)
            .WithEventDataBuilder(_streamReader)
            .BuildAsync(context.CancellationToken);

        var storeCount = state.ZoneVisitor.Sum(x => x.Value);

        // Should we emit some new event to the stores stream?
        _logger.LogWarning("Adjust visitor count for store {Store} to be : {TotalVisitors}", message.Store, storeCount);

        var evt = new StoreVisitorsAdjustedEvent
        {
            Store = context.Message.Store,
            VisitorsAfterAdjustment = storeCount,
            AdjustmentEventId = context.CorrelationId?.ToString(),
            Reason = $"Adjusting total count for store {message.Store} since zone {message.Zone} was reset",
            Timestamp = message.Timestamp
        };

        var allStoresStream = $"stores-{message.Timestamp.Date:yyyy-MM-dd}";
        await _eventWriter.WriteEventAsync(allStoresStream, evt);
    }
}