namespace ES.Labs.RetailRhythmRadar.StoreFlow.Projections;

public record StoreState(string Store, int CurrentCount, int MaxCapacity);