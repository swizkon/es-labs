using EventSourcing;
using EventSourcing.EventStoreDB;
using EventStore.Client;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using RetailRhythmRadar.Domain.Commands;
using RetailRhythmRadar.Domain.Events;
using RetailRhythmRadar.Domain.Handlers;
using RetailRhythmRadar.Domain.Projections;
using RetailRhythmRadar.Domain.Queries;

namespace RetailRhythmRadar.Configuration;

public static class Setup
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEventStoreClient(_ => EventStoreClientSettings
            .Create(configuration.GetConnectionString("EVENTSTORE") ?? string.Empty));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("REDIS");
            options.InstanceName = "ESDemo-";
        });

        services.AddSingleton<IEnrichMetaData, MetadataEnricher>();
        services.AddSingleton<IWriteEvents, EventStoreDbStreamReader>();
        services.AddSingleton<IReadStreams, EventStoreDbStreamReader>();

        services.AddMassTransit(x =>
        {
            x.AddConsumersFromNamespaceContaining<ZoneHandler>();

            x.SetKebabCaseEndpointNameFormatter();

            x.UsingGrpc((context, cfg) =>
            {
                cfg.Host(h =>
                {
                    h.Host = "127.0.0.1";
                    h.Port = 19796;
                });

                cfg.ConfigureEndpoints(context);
            });
        });
    }

    public static WebApplication MapCommandEndPoints(this WebApplication application)
    {
        var bus = application.Services.GetRequiredService<IBus>();

        var commandRoutes = application.MapGroup("commands");

        foreach (var command in typeof(Setup).Assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false } && t.IsAssignableTo(typeof(ZoneCommand))))
        {
            commandRoutes.MapPost(command.Name, (Func<HttpContext, Task<IActionResult>>)(async payload =>
            {
                var p = await payload.Request.ReadFromJsonAsync(command);
                await bus.Publish(p!);
                return new OkResult();
            }));
        }
        return application;
    }

    public static WebApplication MapEventEndPoints(this WebApplication application)
    {
        var bus = application.Services.GetRequiredService<IBus>();

        application
            .MapGroup("events")
            .MapPost(nameof(TurnstilePassageDetected), (Func<TurnstilePassageDetected, Task<IActionResult>>)(async ([FromBody] message) =>
            {
                await bus.Publish(message);
                return new OkResult();
            }));
        return application;
    }

    public static WebApplication MapQueryEndPoints(this WebApplication application)
    {
        var bus = application.Services.GetRequiredService<IBus>();

        var queryRoutes = application.MapGroup("queries");

        queryRoutes.MapGet("stores/{date}", (Func<string, Task<AllStoresProjection>>)(async ([FromRoute] date) =>
        {
            var d = GetDate(date);
            var response = await bus.Request<GetStores, AllStoresProjection>(new GetStores(d));
            return response.Message;
        }));

        queryRoutes.MapGet("store-{store}/{date}", (Func<string, string, Task<SingleStoreState>>)(async ([FromRoute] store, [FromRoute] date) =>
        {
            var d = GetDate(date);
            var response = await bus.Request<GetStore, SingleStoreState>(new GetStore(store, d));
            return response.Message;
        }));
        return application;
    }

    private static DateTime GetDate(string dateAsString)
    {
        if (!DateTime.TryParse(dateAsString, out var date))
        {
            date = DateTime.UtcNow.Date;
        }

        return date;
    }
}