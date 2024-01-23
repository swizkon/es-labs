using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using EventSourcing;
using EventSourcing.EventStoreDB;
using EventStore.Client;
using MassTransit;
using RetailRhythmRadar.Domain.Events;
using RetailRhythmRadar.Domain.Projections;

namespace RetailRhythmRadar.BackgroundServices;

public class ConsumerHostedService : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<ConsumerHostedService> _logger;

    private volatile bool _eventStoreReady;

    private static readonly ConcurrentDictionary<string, AverageTimeProjection> States = new();

    private Subject<AverageTimeProjection>? _projectionSubscription;
    private IDisposable? _projectionStreamS;
    private readonly IConfiguration _configuration;
    private readonly IBus _bus;

    public ConsumerHostedService(IServiceProvider serviceProvider, IHostApplicationLifetime lifetime, ILogger<ConsumerHostedService> logger)
    {
        _lifetime = lifetime;
        _logger = logger;
        _configuration = serviceProvider.GetRequiredService<IConfiguration>();
        _bus = serviceProvider.GetRequiredService<IBus>();
    }

    private static string StreamName => $"stores-{DateTime.UtcNow:yyyy-MM-dd}";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Starting background service...");
        if (!await WaitForAppStartup(_lifetime, stoppingToken))
        {
            return;
        }

        _logger.LogInformation($"Continue...");
        
        while (!_eventStoreReady)
        {
            _eventStoreReady = await WaitForEventStore(_lifetime, stoppingToken);
            if (!_eventStoreReady)
            {
                _logger.LogInformation($"App not started. Wait for 1 sec...");
                await Task.Delay(1_000, stoppingToken);
            }
        }

        //var enters = new Subject<StoreEnteredEvent>();
        //var exits = new Subject<StoreExitedEvent>();

        //var aggr = enters.Zip(exits)
        //    .Throttle(100.Milliseconds())
        //    .Select(tuple => tuple.First.Timestamp - tuple.Second.Timestamp)
        //    .Subscribe(tuple => 
        //    {

        //    });

        _projectionSubscription = new Subject<AverageTimeProjection>();
        _projectionStreamS = _projectionSubscription
            .Throttle(50.Milliseconds())
            .Subscribe(async state =>
            {
                Console.WriteLine(state);
                try
                {
                    await _bus.Publish(state, stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message, e);
                }
            });

        await MainAsync(_projectionSubscription, stoppingToken);
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
                var eventData = ResolveEvent(e, eventResolver).EventData;
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

        _logger.LogInformation("Subscribed to stream " + subscription.SubscriptionId);
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

    private async Task<bool> WaitForAppStartup(IHostApplicationLifetime lifetime, CancellationToken stoppingToken)
    {
        var startedSource = new TaskCompletionSource();
        var cancelledSource = new TaskCompletionSource();

        using var reg1 = lifetime.ApplicationStarted.Register(() => startedSource.SetResult());
        using var reg2 = stoppingToken.Register(() => cancelledSource.SetResult());

        var completedTask = await Task.WhenAny(
            startedSource.Task,
            cancelledSource.Task);

        return completedTask == startedSource.Task;
    }

    private async Task<bool> WaitForEventStore(IHostApplicationLifetime lifetime, CancellationToken stoppingToken)
    {
        var client = EventStoreDbUtils.GetDefaultClient(_configuration.GetConnectionString("EVENTSTORE")!);

        try
        {
            var meta = await client.GetStreamMetadataAsync(StreamName, cancellationToken: stoppingToken);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            return false;
        }
    }
}