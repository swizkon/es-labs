namespace RetailRhythmRadar.StoreFlow;

public record StoreZoneState(
    string Zone,
    int CurrentCount,
    int Max,
    int Min);