using System.Diagnostics;
using Common.Extensions;
using ES.Labs.RetailRhythmRadar.StoreFlow.Commands;
using EventSourcing;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;

namespace ES.Labs.RetailRhythmRadar.StoreFlow.Handlers;

public class ZoneHandler // :
        // IConsumer<EnteringZone>,
        // IConsumer<LeavingZone>,
        // IConsumer<GetZoneState>
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

    //public async Task Consume(ConsumeContext<EnteringZone> context)
    //{
    //    var evt = new EnterZoneAccepted
    //    {
    //        Zone = context.Message.Zone,
    //        Timestamp = context.Message.Timestamp
    //    };

    //    await _eventWriter.WriteEventAsync($"zone-{context.Message.Zone}", evt);
    //}

    //public async Task Consume(ConsumeContext<LeavingZone> context)
    //{
    //    var evt = new LeaveZoneRegistered
    //    {
    //        Zone = context.Message.Zone,
    //        Timestamp = context.Message.Timestamp
    //    };

    //    await _eventWriter.WriteEventAsync($"zone-{context.Message.Zone}", evt);
    //}

    //public async Task Consume(ConsumeContext<GetZoneState> context)
    //{
    //    var streamName = $"zone-{context.Message.Zone}";
    //    var projection = await _cache.GetOrSetAsync(streamName, () => new StoreZoneProjection().WithZone(context.Message.Zone));

    //    _logger.LogInformation($"Projection start at {projection.Revision ?? 0}");

    //    var timer = Stopwatch.StartNew();

    //    var events = _streamReader.ReadEventsAsync(streamName, revision: projection.Revision, cancellationToken: context.CancellationToken);

    //    projection = await events.AggregateAsync(
    //        seed: projection,
    //        accumulator:(current, e) =>
    //            current
    //                .ApplyEvent(e.EventData)
    //                .WithRevision(e.Revision),
    //        cancellationToken: context.CancellationToken);

    //    timer.Stop();

    //    _logger.LogInformation($"Projection is now at {projection.Revision} after {timer.ElapsedMilliseconds} ms");

    //    await _cache.SetAsync(streamName, () => projection);

    //    await context.RespondAsync(projection);
    //}
}