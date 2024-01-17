namespace RetailRhythmRadar.StoreFlow.Events;

public class StoreVisitorsAdjustedEvent : StoreDomainEvent
{
    public required string AdjustmentEventId { get; set; }
    public required string Reason { get; set; }
    public required int VisitorsAfterAdjustment { get; set; }
}