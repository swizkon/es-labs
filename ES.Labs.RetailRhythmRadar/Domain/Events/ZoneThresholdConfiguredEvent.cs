namespace RetailRhythmRadar.Domain.Events;

public class ZoneThresholdConfiguredEvent
{
    public required string Zone { get; set; }
    public required int Threshold { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}