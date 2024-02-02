namespace RetailRhythmRadar.Domain.Events;

public class ZoneManuallyAdjustedEvent : ZoneDomainEvent
{
    public required string Who { get; set; }
    public required string Reason { get; set; }
    public required int NumberOfVisitors { get; set; }
}