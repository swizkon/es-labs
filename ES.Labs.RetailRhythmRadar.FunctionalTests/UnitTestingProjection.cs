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
        var randomTime = GetRandomTime();
        FivePeopleEnteredAtSameTime(randomTime)

        // Act
        .PeopleExitedAt(randomTime.AddSeconds(10))

        // Assert
        .AverageTime.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void It_should_calculate_avg_time_per_customer()
    {
        // Given
        var randomTime = GetRandomTime();
        FivePeopleEnteredAtSameTime(randomTime)

        // When
        .PeopleExitedAt(
            randomTime.AddSeconds(10),
            randomTime.AddSeconds(20),
            randomTime.AddSeconds(36))
        
        // Then
        .AverageTime.Should().Be(TimeSpan.FromSeconds(22));
    }

    private static  AverageTimeProjection FivePeopleEnteredAtSameTime(DateTime timestamp)
    {
        var projection = Enumerable.Range(1, 5)
            .Aggregate(AverageTimeProjection.Empty,
                (current, _) => current.ApplyEvent(new StoreEnteredEvent { Store = string.Empty, Timestamp = timestamp }));

        return projection; //, time);
    }

    private static DateTime GetRandomTime()
    {
        return DateTime.UtcNow.Date.AddMinutes(-Random.Shared.Next(10, 1000));
    }
}

internal static class TestSetup
{
    private static AverageTimeProjection PeopleEnteredAt(this AverageTimeProjection projection, params DateTime[] timestamps)
    {
        return timestamps
            .Aggregate(projection,
                (current, ts) => current.ApplyEvent(new StoreEnteredEvent { Store = string.Empty, Timestamp = ts }));
    }

    public static AverageTimeProjection PeopleExitedAt(this AverageTimeProjection projection, params DateTime[] timestamps)
    {
        return timestamps
            .Aggregate(projection,
                (current, ts) => current.ApplyEvent(new StoreExitedEvent { Store = string.Empty, Timestamp = ts }));

    }
}