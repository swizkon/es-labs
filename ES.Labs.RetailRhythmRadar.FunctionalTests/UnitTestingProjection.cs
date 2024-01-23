using FluentAssertions;
using RetailRhythmRadar.Domain.Events;
using RetailRhythmRadar.Domain.Projections;

namespace ES.Labs.RetailRhythmRadar.FunctionalTests;

public class UnitTestingProjection
{
    [Fact]
    public void It_should_calculate_avg_time_for_single_customer()
    {
        // Arrange
        var (projection, time) = FivePeopleEnteredAtSameTime();

        // Act
        projection = projection.PeopleExitedAt(time.AddSeconds(10));

        // Assert
        projection.AverageTime.Should().Be(TimeSpan.FromSeconds(10));
        projection.CurrentNumberOfVisitors.Should().Be(4);
    }

    [Fact]
    public void It_should_calculate_avg_time_per_customer()
    {
        // Arrange
        var (projection, time) = FivePeopleEnteredAtSameTime();

        // Act
        projection.PeopleExitedAt(time.AddSeconds(10), time.AddSeconds(20), time.AddSeconds(36))
        
        // Assert
        .AverageTime.Should().Be(TimeSpan.FromSeconds(22));
    }

    private static (AverageTimeProjection, DateTime) FivePeopleEnteredAtSameTime()
    {
        var time = DateTime.UtcNow.Date.AddMinutes(-Random.Shared.Next(10, 1000));

        var projection = Enumerable.Range(1, 5)
            .Aggregate(AverageTimeProjection.Empty,
                (current, _) => current.ApplyEvent(new StoreEnteredEvent { Store = string.Empty, Timestamp = time }));

        return (projection, time);
    }
}

internal static class TestSetup
{
    public static AverageTimeProjection PeopleExitedAt(this AverageTimeProjection projection, params DateTime[] timestamps)
    {
        return timestamps
            .Aggregate(projection,
                (current, ts) => current.ApplyEvent(new StoreExitedEvent { Store = string.Empty, Timestamp = ts }));

    }
}