using System.Diagnostics;
using Common.Extensions;
using EventSourcing;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using RetailRhythmRadar.StoreFlow.Projections;
using RetailRhythmRadar.StoreFlow.Queries;

namespace RetailRhythmRadar.StoreFlow.Handlers;

public class ProjectionsHandler : IConsumer<GetStores>, IConsumer<GetStore>
{
    private readonly IReadStreams _eventStreams;
    //private readonly EventDataBuilder _eventDataBuilder;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ProjectionsHandler> _logger;

#pragma warning disable IDE0290
    public ProjectionsHandler(// EventDataBuilder eventDataBuilder,
        IReadStreams eventStreams,
        IDistributedCache cache,
        ILogger<ProjectionsHandler> logger)
#pragma warning restore IDE0290
    {
        // Keep this since the test setup does not seem to work correctly primary constructors
        _eventStreams = eventStreams;
        _cache = cache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GetStores> context)
    {
        var streamName = $"stores-{context.Message.Date:yyyy-MM-dd}";
        var projection = await _cache.GetOrSetAsync(streamName, () => new AllStoresProjection().WithDate(context.Message.Date));

        _logger.LogInformation($"Projection {streamName} start at {projection.Revision ?? 0}");

        var timer = Stopwatch.StartNew();
        projection = await projection.Rehydrate(_eventStreams, context.CancellationToken);
        timer.Stop();

        _logger.LogInformation($"Projection {streamName} is now at {projection.Revision ?? 0} after {timer.ElapsedMilliseconds} ms");

        await _cache.SetAsync(streamName, () => projection);

        await context.RespondAsync(projection);
    }

    public async Task Consume(ConsumeContext<GetStore> context)
    {
        var state = await new SingleStoreProjection(context.Message.Store, context.Message.Date)
            .WithCache(_cache)
            .WithEventDataBuilder(_eventStreams)
            .BuildAsync(context.CancellationToken);

        await context.RespondAsync(state);
    }
}