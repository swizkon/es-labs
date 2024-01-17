namespace RetailRhythmRadar.StoreFlow.Events;

public abstract class StoreDomainEvent
{
    public required string Store { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}