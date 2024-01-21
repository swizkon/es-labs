﻿using RetailRhythmRadar.Domain.Events;

namespace RetailRhythmRadar.Domain.Projections;

public record AverageTimeProjection(
    string Store,
    int CurrentNumberOfVisitors,
    int TotalOfVisitors,
    TimeSpan TotalTime,
    TimeSpan AverageTime,
    IEnumerable<DateTime> Entries)
{
    public static AverageTimeProjection Empty => new("1", 0, 0, TimeSpan.Zero, TimeSpan.Zero, new List<DateTime>());

    public AverageTimeProjection ApplyEvent(StoreEnteredEvent entered)
    {
        return this with
        {
            TotalOfVisitors = TotalOfVisitors + 1,
            CurrentNumberOfVisitors = CurrentNumberOfVisitors + 1,
            Entries = Entries.Append(entered.Timestamp).ToList()
        };
    }

    public AverageTimeProjection ApplyEvent(StoreExitedEvent exited)
    {
        if (CurrentNumberOfVisitors < 0)
            return Empty;

        var entries = Entries.ToList();
        
        // Get the timespan between now and the last time the store was entered
        var timeSpan = exited.Timestamp - entries.LastOrDefault();

        var totalTime = TotalTime + timeSpan;

        return this with
        {
            CurrentNumberOfVisitors = CurrentNumberOfVisitors - 1,
            TotalTime = totalTime,
            AverageTime = TimeSpan.FromTicks(totalTime.Ticks / Math.Max(TotalOfVisitors, 1)),
            Entries = Entries.SkipLast(1).ToList()
        };
    }
}