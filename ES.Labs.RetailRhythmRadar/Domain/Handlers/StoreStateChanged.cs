namespace RetailRhythmRadar.Domain.Handlers;

public class StoreStateChanged
{
    public required string Store { get; set; }
    public required string[] Zones { get; set; }
    public required bool StoreChanged { get; set; }
    public required DateTime Date { get; set; }
}