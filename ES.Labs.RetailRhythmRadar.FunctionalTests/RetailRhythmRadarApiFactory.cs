using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using EventSourcing.EventStoreDB;
using EventStore.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RetailRhythmRadar;
using Testcontainers.Redis;

namespace ES.Labs.RetailRhythmRadar.FunctionalTests;

public class RetailRhythmRadarApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly RedisContainer redis = new RedisBuilder().Build();

    private readonly IContainer esContainer = new ContainerBuilder()
        .WithImage("eventstore/eventstore:latest") // This should be pinned...

        .WithPortBinding(2113, true)
        .WithPortBinding(1113, true)

        .WithEnvironment("EVENTSTORE_EXT_TCP_PORT", "1113")
        .WithEnvironment("EVENTSTORE_RUN_PROJECTIONS", "all")
        .WithEnvironment("EVENTSTORE_START_STANDARD_PROJECTIONS", "true")
        .WithEnvironment("PROJECTION_THREADS", "8")
        .WithEnvironment("INSECURE", "true")
        .WithEnvironment("EVENTSTORE_INSECURE", "true")
        .WithEnvironment("EVENTSTORE_ENABLE_EXTERNAL_TCP", "true") // Redundant?
        .WithEnvironment("EVENTSTORE_ENABLE_ATOM_PUB_OVER_HTTP", "true") // Redundant?
        // .WithEnvironment("EVENTSTORE_MEM_DB", "true")

        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(2113)))
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseSetting("ConnectionStrings:REDIS", redis.GetConnectionString())
            .UseSetting("ConnectionStrings:EVENTSTORE", $"esdb://admin:changeit@{esContainer.Hostname}:{esContainer.GetMappedPublicPort(2113)}?tls=false&tlsVerifyCert=false")
            .ConfigureTestServices(services =>
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redis.GetConnectionString();
                    options.InstanceName = "ESDemo-";
                });

                // Use in-memory caching for testing
                services.RemoveAll(typeof(IDistributedCache));
                services.AddSingleton<IDistributedCache, MemoryDistributedCache>();
            });
    }

    public async Task InitializeAsync()
    {
        await esContainer.StartAsync();
        await redis.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await esContainer.StopAsync();
        await redis.StopAsync();
        await base.DisposeAsync();
    }

    public TService GetService<TService>() => Services.GetRequiredService<TService>();

    public async Task<ulong?> GetStreamPosition(string streamName)
    {
        var configuration = base.Services.GetRequiredService<IConfiguration>();
        var client = EventStoreDbUtils.GetDefaultClient(configuration.GetConnectionString("EVENTSTORE")!);
        try
        {
            var s = client.ReadStreamAsync(
                direction: Direction.Backwards,
                streamName: streamName,
                revision: StreamPosition.End,
                maxCount: 1);
            var state = await s.ReadState;
            if (state == ReadState.StreamNotFound)
            {
                return default;
            }

            var first = await s.FirstOrDefaultAsync();

            return (ulong) first.Event.EventNumber;
        }
        catch (Exception)
        {
            return default;
        }
    }
}