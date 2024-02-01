namespace RetailRhythmRadar.Domain.Projections;

public class SingleStoreState
{
    public DateTime Date { get; set; }

    public ulong? Revision { get; set; }

    public IDictionary<string, int> ZoneVisitor { get; set; } = new Dictionary<string, int>();

    public int ZoneA => ZoneVisitor.FirstOrDefault(z => z.Key == "A").Value;

    public int ZoneB => ZoneVisitor.FirstOrDefault(z => z.Key == "B").Value;

    public int ZoneC => ZoneVisitor.FirstOrDefault(z => z.Key == "C").Value;

    public int ZoneD => ZoneVisitor.FirstOrDefault(z => z.Key == "D").Value;
}