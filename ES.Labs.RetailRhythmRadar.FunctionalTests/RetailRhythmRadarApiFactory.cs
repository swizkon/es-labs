using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
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

        // Wait until the HTTP endpoint of the container is available.
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(2113)))
        // Build the container configuration.
        .Build();

    /*
       eventstore:
       container_name: esdb-docs
       image: eventstore/eventstore:latest
       ports:
       - '2113:2113'
       - '1112:1112'
       - '1113:1113'
       environment:
       EVENTSTORE_EXT_TCP_PORT: 1113
       EVENTSTORE_RUN_PROJECTIONS: all
       EVENTSTORE_START_STANDARD_PROJECTIONS: 'true'
       PROJECTION_THREADS: 8
       INSECURE: true
       EVENTSTORE_INSECURE: true
       EVENTSTORE_ENABLE_EXTERNAL_TCP: true
       EVENTSTORE_ENABLE_ATOM_PUB_OVER_HTTP: true

       volumes:
       - type: volume
       source: eventstore-volume-data
       target: /var/lib/eventstore
       - type: volume
       source: eventstore-volume-logs
       target: /var/log/eventstore
     */

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
            });
    }

    public async Task InitializeAsync()
    {
        await esContainer.StartAsync();
        //await redisStack.StartAsync();
        await redis.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await esContainer.StopAsync();
        // await redisStack.StopAsync();
        await redis.StopAsync();
        await base.DisposeAsync();
    }
}