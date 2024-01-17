using EventSourcing;
using EventSourcing.EventStoreDB;
using RetailRhythmRadar.StoreFlow.Events;

namespace RetailRhythmRadar.StoreFlow.Projections;

public class AllStoresProjection
{
    public AllStoresProjection WithDate(DateTime date)
    {
        Date = date;
        return this;
    }

    public DateTime Date { get; set; }

    public ulong? Revision { get; set; }

    public IDictionary<string, int> StoreVisitor { get; set; } = new Dictionary<string, int>();

    public IEnumerable<StoreState> StoreStates => StoreVisitor.Select(x => new StoreState(x.Key, x.Value, 50));

    public int TotalVisitor => StoreVisitor.Sum(x => x.Value);

    public async Task<AllStoresProjection> Rehydrate(IReadStreams streamReader, CancellationToken cancellationToken)
    {
        var eventTypeResolver = new CustomEventResolver(new DefaultEventResolver());
        var streamName = $"stores-{Date:yyyy-MM-dd}";
        var events = streamReader.ReadEventsAsync(
            streamName: streamName,
            revision: Revision, 
            resolver: eventTypeResolver,
            cancellationToken: cancellationToken);

        return await events.AggregateAsync(
            seed: this,
            accumulator: (current, e) =>
                current
                    .ApplyEvent(e.EventData)
                    .WithRevision(e.Revision),
            cancellationToken: cancellationToken);
    }

    private AllStoresProjection ApplyEvent(object eventData)
    {
        switch (eventData)
        {
            case StoreEnteredEvent entered:
                StoreVisitor[entered.Store] = StoreVisitor.TryGetValue(entered.Store, out var count) ? count + 1 : 1;
                break;

            case StoreExitedEvent exited:
                StoreVisitor[exited.Store] = StoreVisitor.TryGetValue(exited.Store, out var count2) ? count2 - 1 : 0;
                break;

            case StoreVisitorsAdjustedEvent adjusted:
                StoreVisitor[adjusted.Store] = adjusted.VisitorsAfterAdjustment;
                break;

            default:
                Console.WriteLine($"No handler defined for {eventData.GetType()}");
                break;
        }

        return this;
    }

    private AllStoresProjection WithRevision(ulong revision)
    {
        Revision = revision;
        return this;
    }
}