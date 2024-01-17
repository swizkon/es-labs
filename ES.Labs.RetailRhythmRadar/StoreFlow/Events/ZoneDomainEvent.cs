namespace RetailRhythmRadar.StoreFlow.Events;

public abstract class ZoneDomainEvent : StoreDomainEvent
{
    public required string Zone { get; set; }

    public override string ToString()
    {
        return $"{GetType().Name} Store: {Store} Zone: {Zone}";
    }   
}