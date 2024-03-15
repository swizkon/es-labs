using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using Common.Extensions;
using EventSourcing.EventStoreDB;
using EventStore.Client;
using MassTransit;
using RetailRhythmRadar.Domain.Events;
using RetailRhythmRadar.Domain.Projections;

namespace RetailRhythmRadar.BackgroundServices;

public class AnomalyDetectionService : EventStoreSubscriptionBase
{
    private readonly ILogger<AnomalyDetectionService> _logger;

    private static readonly ConcurrentDictionary<string, EnterAndExists> States = new();

    private Subject<EnterAndExists>? _projectionSubscription;
    private IDisposable? _projectionStreamS;
    private readonly IConfiguration _configuration;
    private readonly IBus _bus;

    public AnomalyDetectionService(IServiceProvider serviceProvider, ILogger<AnomalyDetectionService> logger) : base(logger)
    {
        _logger = logger;
        _configuration = serviceProvider.GetRequiredService<IConfiguration>();
        _bus = serviceProvider.GetRequiredService<IBus>();
    }

    private static string StreamName => $"stores-{DateTime.UtcNow:yyyy-MM-dd}";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Starting background service...");
        
        var eventStoreReady = await WaitForEventStore(StreamName, _configuration, TimeSpan.FromDays(14), stoppingToken);

        if (!eventStoreReady)
        {
            _logger.LogInformation("EventStoreDB not available. Wait for 1 sec...");
            return;
        }

        _projectionSubscription = new Subject<EnterAndExists>();
        _projectionStreamS = _projectionSubscription
            .Throttle(50.Milliseconds())
            .Subscribe(async state =>
            {
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

    public async Task MainAsync(Subject<EnterAndExists>? projectionSubscription, CancellationToken cancellationToken)
    {
        var eventResolver = new CustomEventResolver(new DefaultEventResolver(new GreedyEventResolver(Assembly.GetAssembly(GetType()))));
        var client = EventStoreDbUtils.GetDefaultClient(_configuration.GetConnectionString("EVENTSTORE")!);
        
        var subscription = await client.SubscribeToStreamAsync(
            streamName: StreamName,
            start: FromStream.Start,
            // start: FromStream.End,
            eventAppeared: (_, e, _) =>
            {
                var eventData = EventStoreDbUtils.ResolveEvent(e, eventResolver).EventData;
                switch (eventData)
                {
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
    }

    private static void Handle(StoreExitedEvent entered, IObserver<EnterAndExists>? projectionSubscription) =>
        TransformState(
            resolvedEvent: entered,
            projectionSubscription: projectionSubscription,
            modifier: (state, e) => state.ApplyEvent(e));

    private static void TransformState<TEvent>(
        TEvent resolvedEvent,
        IObserver<EnterAndExists>? projectionSubscription,
        Func<EnterAndExists, TEvent, EnterAndExists> modifier)
    {
        var s = States.AddOrUpdate(StreamName,
             modifier(EnterAndExists.InitialState("1"), resolvedEvent),
            (_, state) => modifier(state, resolvedEvent));

        if (s.FrontDoorExits > s.GiftShopExits * 2)
        {
            projectionSubscription?.OnNext(s);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _projectionStreamS?.Dispose();
        _projectionSubscription?.Dispose();

        States.Clear();

        return base.StopAsync(cancellationToken);
    }
}