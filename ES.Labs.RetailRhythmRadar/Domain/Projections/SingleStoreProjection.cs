using Common.Extensions;
using EventSourcing;
using EventSourcing.EventStoreDB;
using Microsoft.Extensions.Caching.Distributed;
using RetailRhythmRadar.Domain.Events;

namespace RetailRhythmRadar.Domain.Projections;

public class SingleStoreProjection
{
    private readonly string _store;
    private IDistributedCache? _cache;
    private IReadStreams? _eventStreams;
    private readonly IEventTypeResolver _eventTypeResolver;

    private SingleStoreState _state = new();

    public SingleStoreProjection(string store, DateTime date)
    {
        _store = store;
        _state.Date = date;
        _eventTypeResolver = new CustomEventResolver(new DefaultEventResolver());
    }

    public SingleStoreProjection WithCache(IDistributedCache cache)
    {
        _cache = cache;
        return this;
    }

    public SingleStoreProjection WithEventDataBuilder(IReadStreams eventDataBuilder)
    {
        _eventStreams = eventDataBuilder;
        return this;
    }

    public async Task<SingleStoreState> BuildAsync(CancellationToken cancellationToken)
    {
        var streamName = $"store-{_store}-{_state.Date:yyyy-MM-dd}";
        _state = await _cache!.GetOrSetAsync(streamName, () => _state);
        _state = await Rehydrate(cancellationToken);
        return await _cache!.SetAsync(streamName, () => _state);
    }

    private async Task<SingleStoreState> Rehydrate(CancellationToken cancellationToken)
    {
        var streamName = $"store-{_store}-{_state.Date:yyyy-MM-dd}";
        var events = _eventStreams.ReadEventsAsync(
            streamName: streamName,
            revision: _state.Revision,
            resolver: _eventTypeResolver,
            cancellationToken: cancellationToken);

        await foreach (var @event in events)
        {
            ApplyEvent(@event.EventData);
            WithRevision(@event.Revision);
        }

        return _state;
    }

    private void ApplyEvent(object eventData)
    {
        switch (eventData)
        {
            case ZoneEnteredEvent entered:
                _state.ZoneVisitor[entered.Zone] = _state.ZoneVisitor.TryGetValue(entered.Zone, out var count) ? count + 1 : 1;
                break;

            case ZoneExitedEvent exited:
                _state.ZoneVisitor[exited.Zone] = _state.ZoneVisitor.TryGetValue(exited.Zone, out var count2) ? count2 - 1 : -1;
                break;

            case ZoneManuallyClearedEvent cleared:
                _state.ZoneVisitor[cleared.Zone] = 0;
                break;

            default:
                Console.WriteLine($"No handler defined for {eventData.GetType()}");
                break;
        }
    }

    private void WithRevision(ulong revision)
    {
        _state.Revision = revision;
    }
}