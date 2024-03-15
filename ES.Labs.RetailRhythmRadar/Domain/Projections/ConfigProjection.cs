using Common.Extensions;
using EventSourcing;
using EventSourcing.EventStoreDB;
using Microsoft.Extensions.Caching.Distributed;
using RetailRhythmRadar.Domain.Events;
using System.Reflection;

namespace RetailRhythmRadar.Domain.Projections;

public class ConfigProjection
{
    private readonly IDistributedCache _cache;
    private readonly IReadStreams _eventStreams;
    private readonly IEventTypeResolver _eventTypeResolver = new CustomEventResolver(new DefaultEventResolver(new GreedyEventResolver(Assembly.GetExecutingAssembly())));

    private SingleStoreState _state = new();

    private const string StreamName = "configs";

    public ConfigProjection(IReadStreams eventStreams, IDistributedCache cache)
    {
        _eventStreams = eventStreams;
        _cache = cache;
    }

    public async Task<SingleStoreState> BuildAsync(CancellationToken cancellationToken)
    {
        _state = await _cache.GetOrSetAsync(StreamName, () => _state);
        _state = await Rehydrate(cancellationToken);
        return await _cache.SetAsync(StreamName, () => _state);
    }

    private async Task<SingleStoreState> Rehydrate(CancellationToken cancellationToken)
    {
        var events = _eventStreams.ReadEventsAsync(
            streamName: StreamName,
            revision: _state.Revision,
            resolver: _eventTypeResolver,
            cancellationToken: cancellationToken);

        await foreach (var @event in events)
        {
            ApplyEvent(@event.EventData);
            _state.Revision = @event.Revision;

            //if (_state.ZoneA * _state.ZoneB * _state.ZoneC * _state.ZoneD != 0)
            //{
            //    break;
            //}
        }

        return _state;
    }

    private void ApplyEvent(object eventData)
    {
        switch (eventData)
        {
            case ZoneThresholdConfiguredEvent thresholdConfigured:
                _state.ZoneVisitor[thresholdConfigured.Zone] = thresholdConfigured.Threshold;
                break;

            default:
                Console.WriteLine($"No handler defined for {eventData.GetType()}");
                break;
        }
    }
}