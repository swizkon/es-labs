using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using EventSourcing;
using EventSourcing.EventStoreDB;
using EventStore.Client;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using RetailRhythmRadar.Domain.Events;
using RetailRhythmRadar.Domain.Projections;

namespace RetailRhythmRadar.BackgroundServices;

public class ConsumerHostedService(IServiceProvider serviceProvider) : BackgroundService
{
    private static readonly ConcurrentDictionary<string, AverageTimeProjection> States = new();

    private Subject<AverageTimeProjection>? _projectionSubscription;
    private IDisposable? _projectionStreamS;
    private readonly IConfiguration _configuration = serviceProvider.GetRequiredService<IConfiguration>();
    private readonly IBus _bus = serviceProvider.GetRequiredService<IBus>();
    
    private static string StreamName => $"stores-{DateTime.UtcNow:yyyy-MM-dd}";

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"{GetType().Name} Starting...");

        Console.WriteLine($"{GetType().Name} Wait for 10 secs...");

        Task.Delay(10000, stoppingToken);

        Console.WriteLine($"{GetType().Name} Continue...");

        _projectionSubscription = new Subject<AverageTimeProjection>();
        _projectionStreamS = _projectionSubscription
            .Throttle(TimeSpan.FromMilliseconds(50))
            .Subscribe(async state =>
            {
                Console.WriteLine(state);
                try
                {
                    await _bus.Publish(state, stoppingToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
        
        return MainAsync(_projectionSubscription, stoppingToken);
    }

    public async Task MainAsync(Subject<AverageTimeProjection>? projectionSubscription, CancellationToken cancellationToken)
    {
        var eventResolver = new CustomEventResolver(new DefaultEventResolver(new GreedyEventResolver()));
        var client = EventStoreDbUtils.GetDefaultClient(_configuration.GetConnectionString("EVENTSTORE")!);
        
        var subscription = await client.SubscribeToStreamAsync(
            streamName: StreamName,
            // start: FromStream.Start,
            start: FromStream.End,
            eventAppeared: (_, e, _) =>
            {
                var resolveType = ResolveEvent(e, eventResolver);
                var eventData = resolveType.EventData;
                switch (eventData)
                {
                    case StoreEnteredEvent entered:
                        Handle(entered, projectionSubscription);
                        break;

                    case StoreExitedEvent exited:
                        Handle(exited, projectionSubscription);
                        break;

                    default:
                        Console.WriteLine($"No handler defined for {eventData.GetType()}");
                        break;
                }

                return Task.CompletedTask;
            }, cancellationToken: cancellationToken);

        Console.WriteLine("Subscribed to stream " + subscription.SubscriptionId);
        //if(Environment.UserInteractive)
        //    Console.ReadKey();
    }

    private static void Handle(StoreEnteredEvent entered, IObserver<AverageTimeProjection>? projectionSubscription) =>
        TransformState(
            resolvedEvent: entered,
            projectionSubscription: projectionSubscription,
            modifier: (state, e) =>
            {
                state = state.ApplyEvent(e);
                return state;
            });

    private static void Handle(StoreExitedEvent entered, IObserver<AverageTimeProjection>? projectionSubscription) =>
        TransformState(
            resolvedEvent: entered,
            projectionSubscription: projectionSubscription,
            modifier: (state, e) => state.ApplyEvent(e));

    private static void TransformState<TEvent>(
        TEvent resolvedEvent,
        IObserver<AverageTimeProjection>? projectionSubscription,
        Func<AverageTimeProjection, TEvent, AverageTimeProjection> modifier)
    {
        var s = States.AddOrUpdate(StreamName,
             modifier(AverageTimeProjection.Empty, resolvedEvent),
            (_, state) => modifier(state, resolvedEvent));

        projectionSubscription?.OnNext(s);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"{GetType().Name} Stopping...");
        _projectionStreamS?.Dispose();
        _projectionSubscription?.Dispose();

        States.Clear();

        return base.StopAsync(cancellationToken);
    }

    private static DomainEvent ResolveEvent(ResolvedEvent evt, IEventTypeResolver eventResolver)
    {
        var metadata = JsonSerializer.Deserialize<IDictionary<string, string>>(
                           Encoding.UTF8.GetString(evt.Event.Metadata.ToArray()))
                       ?? new Dictionary<string, string>();

        var eType = eventResolver.ResolveType(metadata);

        return new DomainEvent(
            EventType: eType?.FullName ?? string.Empty,
            EventData: GetRecordedEvent(evt.Event, eType!),
            Revision: evt.Event.EventNumber);
    }

    private static object GetRecordedEvent(EventRecord evt, Type type)
    {
        var data = Encoding.UTF8.GetString(evt.Data.Span);
        return JsonSerializer.Deserialize(data, type)!;
    }
}