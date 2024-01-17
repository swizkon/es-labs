using ES.Labs.RetailRhythmRadar.StoreFlow.Commands;
using ES.Labs.RetailRhythmRadar.StoreFlow.Events;
using ES.Labs.RetailRhythmRadar.StoreFlow.Handlers;
using ES.Labs.RetailRhythmRadar.StoreFlow.Projections;
using ES.Labs.RetailRhythmRadar.StoreFlow.Queries;
using EventSourcing;
using EventSourcing.EventStoreDB;
using EventStore.Client;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace ES.Labs.RetailRhythmRadar.StoreFlow;

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

    public static void AddStoreFlowCommands(this WebApplication application)
    {
        var bus = application.Services.GetRequiredService<IBus>();

        var g = application.MapGroup("StoreFlow/commands");

        foreach (var command in typeof(Setup).Assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false } && t.IsAssignableTo(typeof(ZoneCommand))))
        {
            g.MapPost(command.Name, (Func<HttpContext, Task<IActionResult>>)(async payload =>
            {
                var p = await payload.Request.ReadFromJsonAsync(command);
                await bus.Publish(p!);
                return new OkResult();
            }));
        }
    }

    public static void AddStoreFlowEvents(this WebApplication application)
    {
        var bus = application.Services.GetRequiredService<IBus>();

        var eventEndPoints = application.MapGroup("StoreFlow/events");

        eventEndPoints.MapPost(nameof(TurnstilePassageDetected), (Func<TurnstilePassageDetected, Task<IActionResult>>)(async ([FromBody] message) =>
        {
            await bus.Publish(message);
            return new OkResult();
        }));
    }

    public static void AddStoreFlowQueries(this WebApplication application)
    {
        var bus = application.Services.GetRequiredService<IBus>();

        var mapGroup = application.MapGroup("StoreFlow/queries");

        //mapGroup.MapGet("state/{zone}/{date}", (Func<string, string, Task<StoreZoneProjection>>)(async ([FromRoute] zone) =>
        //{

        //    var response = await bus.Request<GetStore, SingleStoreState>(new GetStore(store, d));

        //    return response.Message;

        //    var response = await bus.Request<GetZoneState, StoreZoneProjection>(new GetZoneState(zone));

        //    return response.Message;
        //}));

        mapGroup.MapGet("stores/{date}", (Func<string, Task<AllStoresProjection>>)(async ([FromRoute] date) =>
        {
            if (!DateTime.TryParse(date, out var d))
            {
                d = DateTime.UtcNow.Date;
            }
            var response = await bus.Request<GetStores, AllStoresProjection>(new GetStores(d));

            return response.Message;
        }));

        mapGroup.MapGet("store-{store}/{date}", (Func<string, string, Task<SingleStoreState>>)(async ([FromRoute] store, [FromRoute] date) =>
        {
            if (!DateTime.TryParse(date, out var d))
            {
                d = DateTime.UtcNow.Date;
            }

            var response = await bus.Request<GetStore, SingleStoreState>(new GetStore(store, d));

            return response.Message;
        }));
    }
}