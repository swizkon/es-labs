namespace ES.Labs.RetailRhythmRadar.StoreFlow.Projections;

public class SingleStoreState
{
    public DateTime Date { get; set; }

    public ulong? Revision { get; set; }

    public IDictionary<string, int> ZoneVisitor { get; set; } = new Dictionary<string, int>();
}