namespace RetailRhythmRadar.Domain.Commands;

public class AdjustZone : ZoneCommand
{
    public required string Who { get; set; }
    public required string Reason { get; set; }
    public required int NumberOfVisitors { get; set; }
}