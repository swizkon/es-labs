namespace RetailRhythmRadar.Domain.Events;

public class ZoneManuallyClearedEvent : ZoneDomainEvent
{
    public required string Who { get; set; }
    public required string Reason { get; set; }
    public required int VisitorsBeforeReset { get; set; }
}