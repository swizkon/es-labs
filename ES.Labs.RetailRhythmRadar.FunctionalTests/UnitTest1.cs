using System.Net.Http.Json;
using FluentAssertions;
using RetailRhythmRadar.Domain.Events;
using RetailRhythmRadar.Domain.Projections;
using TestHelpers;

namespace ES.Labs.RetailRhythmRadar.FunctionalTests;

[TestCaseOrderer("TestHelpers.FactSequenceOrderer", "TestHelpers")]
//[Collection("Some test case in sequence")]
public class UnitTest1(RetailRhythmRadarApiFactory apiFactory) : IClassFixture<RetailRhythmRadarApiFactory>
{
    private readonly HttpClient _httpClient = apiFactory.CreateClient();

    [FactSequence(1)]
    public async Task A__It_should_be_possible_to_enter_store()
    {
        var sleep = Task.Delay(1_000);

        var evt = new TurnstilePassageDetected
        {
            Turnstile = new TurnstileIdentifier { SerialNumber = "1-0A"},
            Timestamp = DateTime.UtcNow,
            Direction = TurnstileDirection.Clockwise
        };

        var result = await _httpClient.PostAsJsonAsync("events/TurnstilePassageDetected", evt);

        result.Should().NotBeNull(); //.Contain("The message is someStuff");
        await sleep;
    }

    [FactSequence(2)]
    public async Task B__projection_should_exist()
    {
        var sleep = Task.Delay(1_000);
        var result = await _httpClient.GetFromJsonAsync<SingleStoreState>($"queries/store-1/2024-01-22");

        await sleep;
        result.Date.Should().BeAfter(DateTime.UtcNow.AddDays(-1));
    }

    [FactSequence(3)]
    public async Task C__It_should_be_possible_to_leave_store()
    {
        var sleep = Task.Delay(999);

        var evt = new TurnstilePassageDetected
        {
            Turnstile = new TurnstileIdentifier { SerialNumber = "1-0A" },
            Timestamp = DateTime.UtcNow,
            Direction = TurnstileDirection.CounterClockwise
        };
        var result = await _httpClient.PostAsJsonAsync("events/TurnstilePassageDetected", evt);

        await sleep;
        result.Should().NotBeNull(); //.Contain("The message is someStuff");
    }

    [FactSequence(4)]
    public async Task D__projection_should_exist()
    {
        var sleep = Task.Delay(999);
        var result = await _httpClient.GetFromJsonAsync<AllStoresProjection>($"queries/stores/2024-01-22");

        await sleep;
        result.Date.Should().BeAfter(DateTime.UtcNow.AddDays(-1));
    }

    /*
    */
}