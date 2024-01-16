using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using ES.Labs.Domain;
using ES.Labs.Domain.Projections;
using EventStore.Client;

namespace ES.Labs.Api.BackgroundServices;

public class ConsumerHostedService(HttpClient httpClient, IServiceProvider serviceProvider) : BackgroundService
{
    private static readonly ConcurrentDictionary<string, EqualizerState> EqStates = new();
    private Subject<EqualizerState>? _projectionSubscription;
    private IDisposable? _projectionStreamS;
    private readonly IConfiguration _configuration = serviceProvider.GetRequiredService<IConfiguration>();

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("ExecuteAsync");
        _projectionSubscription = new Subject<EqualizerState>();
        _projectionStreamS = _projectionSubscription
            .Throttle(TimeSpan.FromMilliseconds(1000))
            .Subscribe(async state =>
            {
                Console.WriteLine("EMIT state " + state);
                try
                {
                    var s = JsonSerializer.Serialize(state);
                    Console.WriteLine(s);

                    var d = new StringContent(s, Encoding.UTF8, "application/json");
                    var res = await httpClient.PostAsync($"https://localhost:6001/projections/{state.DeviceName}", d, stoppingToken);
                    Console.WriteLine(res.StatusCode);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
        
        var eventMetadataInfo = serviceProvider.GetRequiredService<IEventMetadataInfo>();
        var eventDataBuilder = new EventDataBuilder(eventMetadataInfo);
        return MainAsync(_projectionSubscription, eventDataBuilder, stoppingToken);
    }

    public async Task MainAsync(Subject<EqualizerState>? projectionSubscription, EventDataBuilder eventDataBuilder, CancellationToken cancellationToken)
    {
        var client = EventStoreUtil.GetDefaultClient(_configuration.GetConnectionString("EVENTSTORE")!);

        var dd = new EqualizerAggregate(client, eventDataBuilder);
        // await dd.InitStream();
        await dd.Hydrate();

        var subscription = await client.SubscribeToStreamAsync(streamName: EventStoreConfiguration.DeviceStreamName,
            start: FromStream.Start,
            eventAppeared: (_, e, _ct) =>
            {
                if (e.Event.EventType == nameof(Events.ChannelLevelChanged))
                {
                    HandleChannelLevelChanged(e, projectionSubscription);
                }
                else if (e.Event.EventType == nameof(Commands.SetVolume))
                {
                    HandleSetVolume(e, projectionSubscription);
                }
                else
                {
                    Console.WriteLine($"Unknown event type {e.Event.EventType}");
                }

                return Task.CompletedTask;
            }, cancellationToken: cancellationToken);

        Console.WriteLine("Subscribed to stream " + subscription.SubscriptionId);
    }

    private static void HandleChannelLevelChanged(ResolvedEvent resolvedEvent, Subject<EqualizerState>? projectionSubscription)
    {
        TransformState<Events.ChannelLevelChanged>(
            resolvedEvent: resolvedEvent,
            projectionSubscription: projectionSubscription,
            modifier: (state, levelChanged) =>
            {
                state.Channels
                    = state
                        .Channels
                        .Where(c => c.Channel != levelChanged.Channel)
                        .Append(new EqualizerState.EqualizerBandState
                        {
                            Channel = levelChanged.Channel,
                            Level = levelChanged.Level.ToString().PadLeft(2, '0')
                        })
                        .OrderBy(x => x.Channel)
                        .ToList();
            });
    }

    private static void HandleSetVolume(ResolvedEvent resolvedEvent, Subject<EqualizerState>? projectionSubscription)
    {
        TransformState<Commands.SetVolume>(
            resolvedEvent: resolvedEvent,
            projectionSubscription: projectionSubscription,
            modifier: (state, setVolume) =>
            {
                state.Volume = setVolume.Volume;
            });
    }

    private static void TransformState<TEvent>(
        ResolvedEvent resolvedEvent,
        IObserver<EqualizerState>? projectionSubscription,
        Action<EqualizerState, TEvent> modifier)
    {
        var s = EqStates.AddOrUpdate(EventStoreConfiguration.DeviceStreamName,
            new EqualizerState
            {
                DeviceName = EventStoreConfiguration.DeviceStreamName
            },
            (_, state) =>
            {
                state.CurrentVersion = resolvedEvent.OriginalEventNumber;
                modifier(state, EventStoreUtil.GetRecordedEventAs<TEvent>(resolvedEvent.Event));
                return state;
            });

        projectionSubscription?.OnNext(s);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _projectionStreamS?.Dispose();
        _projectionSubscription?.Dispose();

        return base.StopAsync(cancellationToken);
    }
}