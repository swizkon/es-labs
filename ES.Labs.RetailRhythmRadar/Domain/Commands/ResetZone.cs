namespace RetailRhythmRadar.Domain.Commands;

public class ResetZone : ZoneCommand
{
    public required string Who { get; set; }
    public required string Reason { get; set; }
}
