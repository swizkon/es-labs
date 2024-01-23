using System.Net.Http.Json;
using FluentAssertions;
using RetailRhythmRadar.Domain.Events;
using RetailRhythmRadar.Domain.Processors;
using RetailRhythmRadar.Domain.Projections;
using TestHelpers;

namespace ES.Labs.RetailRhythmRadar.FunctionalTests;

[TestCaseOrderer("TestHelpers.FactSequenceOrderer", "TestHelpers")]
//[Collection("Some test case in sequence")]
public class UnitTest1(RetailRhythmRadarApiFactory apiFactory) : IClassFixture<RetailRhythmRadarApiFactory>
{
    private readonly HttpClient _httpClient = apiFactory.CreateClient();

    private static string StreamName => $"store-1-{DateTime.UtcNow:yyyy-MM-dd}";

    [FactSequence(0)]
    public async Task X__Given_setup()
    {
        var initialRevision = await apiFactory.GetStreamPosition(StreamName);
        initialRevision.Should().BeNull();
    }

    [FactSequence(1)]
    public async Task A__It_should_be_possible_to_enter_store()
    {
        var entryEvents = Enumerable
            .Range(1, 50)
            .Select(x => new TurnstilePassageDetected
            {
                Turnstile = new TurnstileIdentifier { SerialNumber = "1-0A" },
                Timestamp = DateTime.UtcNow.Date.AddHours(8).AddMinutes(x),
                Direction = TurnstileDirection.Clockwise
            })
            .ToList();

        var exitEvents = entryEvents
            .Take(30)
            .Select(x => new TurnstilePassageDetected
            {
                Turnstile = x.Turnstile,
                Timestamp = x.Timestamp.AddMinutes(Random.Shared.Next(1, 30)),
                Direction = TurnstileDirection.CounterClockwise
            });

        var s = apiFactory.GetService<IProcess<TurnstilePassageDetected>>();

        var setupTasks = entryEvents.Concat(exitEvents).Select(x => s.Handle(x));

        Parallel.ForEach(setupTasks, async x => await x);

        var evt = new TurnstilePassageDetected
        {
            Turnstile = new TurnstileIdentifier { SerialNumber = "1-0A"},
            Timestamp = DateTime.UtcNow,
            Direction = TurnstileDirection.Clockwise
        };

        var result = await _httpClient.PostAsJsonAsync("events/TurnstilePassageDetected", evt);
        
        var initialRevision = await apiFactory.GetStreamPosition(StreamName);

        initialRevision.Should().Be(80); 

        result.Should().NotBeNull();
    }

    [FactSequence(2)]
    public async Task B__projection_should_exist()
    {
        var result = await _httpClient.GetFromJsonAsync<SingleStoreState>($"queries/store-1/2024-01-22");

        result.Date.Should().BeAfter(DateTime.UtcNow.AddDays(-1));
    }

    [FactSequence(3)]
    public async Task C__It_should_be_possible_to_leave_store()
    {
        var evt = new TurnstilePassageDetected
        {
            Turnstile = new TurnstileIdentifier { SerialNumber = "1-0A" },
            Timestamp = DateTime.UtcNow,
            Direction = TurnstileDirection.CounterClockwise
        };
        var result = await _httpClient.PostAsJsonAsync("events/TurnstilePassageDetected", evt);

        result.Should().NotBeNull();
    }

    [FactSequence(4)]
    public async Task D__projection_should_exist()
    {
        var result = await _httpClient.GetFromJsonAsync<AllStoresProjection>($"queries/stores/2024-01-22");
        result.Date.Should().BeAfter(DateTime.UtcNow.AddDays(-1));
    }
}