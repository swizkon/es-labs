using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using ES.Labs.Domain;
using ES.Labs.Domain.Projections;
using EventStore.Client;
using Newtonsoft.Json;

namespace ES.Labs.Api.BackgroundServices;

public class ConsumerHostedService(HttpClient httpClient) : BackgroundService
{
    private static readonly ConcurrentDictionary<string, EqualizerState> EqStates = new();
    private Subject<EqualizerState> _projectionSubscription;
    private IDisposable _projectionStreamS;

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
                    var s = JsonConvert.SerializeObject(state);
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

        return MainAsync(_projectionSubscription, stoppingToken);
    }

    public static async Task MainAsync(Subject<EqualizerState> projectionSubscription, CancellationToken cancellationToken)
    {
        var client = EventStoreUtil.GetDefaultClient();

        var dd = new EqualizerAggregate(client);
        await dd.Hydrate();
        
        await client.SubscribeToStreamAsync(EventStoreConfiguration.DeviceStreamName,
            async (subscription, e, cancellationToken) =>
            {
                if (e.Event.EventType == nameof(Events.ChannelLevelChanged))
                {
                    await HandleChannelLevelChanged(e, projectionSubscription);
                }
                else if (e.Event.EventType == nameof(Commands.SetVolume))
                {
                    await HandleSetVolume(e, projectionSubscription);
                }
                else
                {
                    Console.WriteLine($"Unknown event type {e.Event.EventType}");
                }
            }, cancellationToken: cancellationToken);
    }

    private static async Task HandleChannelLevelChanged(ResolvedEvent resolvedEvent, Subject<EqualizerState> projectionSubscription)
    {
        await TransformState<Events.ChannelLevelChanged>(
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

    private static async Task HandleSetVolume(ResolvedEvent resolvedEvent, Subject<EqualizerState> projectionSubscription)
    {
        await TransformState<Commands.SetVolume>(
            resolvedEvent: resolvedEvent,
            projectionSubscription: projectionSubscription,
            modifier: (state, setVolume) =>
            {
                state.Volume = setVolume.Volume;
            });
    }

    private static async Task TransformState<TEvent>(
        ResolvedEvent resolvedEvent,
        Subject<EqualizerState> projectionSubscription,
        Action<EqualizerState, TEvent> modifier)
    {
        var s = EqStates.AddOrUpdate(EventStoreConfiguration.DeviceStreamName,
            new EqualizerState
            {
                DeviceName = EventStoreConfiguration.DeviceStreamName
            },
            (s, state) =>
            {
                state.CurrentVersion = resolvedEvent.OriginalEventNumber;
                modifier(state, EventStoreUtil.GetRecordedEventAs<TEvent>(resolvedEvent.Event));
                return state;
            });

        projectionSubscription.OnNext(s);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _projectionStreamS.Dispose();
        _projectionSubscription.Dispose();

        return base.StopAsync(cancellationToken);
    }
}