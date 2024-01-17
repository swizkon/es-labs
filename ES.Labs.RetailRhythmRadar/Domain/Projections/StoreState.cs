namespace RetailRhythmRadar.Domain.Projections;

public record StoreState(string Store, int CurrentCount, int MaxCapacity);