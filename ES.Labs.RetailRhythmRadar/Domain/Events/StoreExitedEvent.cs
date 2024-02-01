namespace RetailRhythmRadar.Domain.Events;

public class StoreExitedEvent : StoreDomainEvent
{
    public string? Door { get; set; }
}