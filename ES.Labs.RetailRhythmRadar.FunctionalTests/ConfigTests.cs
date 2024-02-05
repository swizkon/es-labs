using System.Net.Http.Json;
using EventSourcing;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using RetailRhythmRadar.Domain.Events;
using RetailRhythmRadar.Domain.Projections;

namespace ES.Labs.RetailRhythmRadar.FunctionalTests;

public class ConfigTests(RetailRhythmRadarApiFactory apiFactory) : IClassFixture<RetailRhythmRadarApiFactory>
{
    private readonly HttpClient _httpClient = apiFactory.CreateClient();

    private static string StreamName => "configs";
    
    [Fact]
    public async Task A__It_should_be_possible_to_enter_store()
    {
        var zones = new[] { "A", "B", "C", "D" };

        var entryEvents = zones
            .SelectMany(x => Enumerable.Range(1, 10)
                .Select(y => new ZoneThresholdConfiguredEvent
            {
                Zone = x,
                Timestamp = DateTime.UtcNow.Date.AddHours(8).AddMinutes(y),
                Threshold = 10
            }))
            .ToList();

        await apiFactory
            .GetService<IWriteEvents>()
            .WriteEventsAsync(StreamName, null, entryEvents);

        var haha = await new ConfigProjection(apiFactory.GetService<IReadStreams>(), apiFactory.GetService<IDistributedCache>())
            .BuildAsync(CancellationToken.None);

        haha.Revision.Should().Be(39);
    }
}