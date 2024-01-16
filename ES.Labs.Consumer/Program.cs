using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using ES.Labs.Domain;
using ES.Labs.Domain.Projections;
using EventStore.Client;
using Microsoft.Extensions.Configuration;

namespace ES.Labs.Consumer;

public class Program
{
    private static readonly ConcurrentDictionary<string, EqualizerState> EqStates = new();

    public static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new List<KeyValuePair<string, string?>>()
            {
                new("ConnectionStrings:EVENTSTORE", "esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false")
            })
            .AddCommandLine(args)
            .AddEnvironmentVariables()
            .Build();

        var httpClient = new HttpClient();

        using var projectionSubscription = new Subject<EqualizerState>();
        var projectionStreamS = projectionSubscription
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Subscribe(state =>
            {
                Console.WriteLine("EMIT state " + state);
                // Send to the api...
                try
                {
                    var d = new StringContent(JsonSerializer.Serialize(state), Encoding.UTF8, "application/json");
                    
                    var res = httpClient.PostAsync($"https://localhost:6001/projections/{state.DeviceName}", d).Result;
                    Console.WriteLine(res.StatusCode);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

        MainAsync(args, projectionSubscription, configuration).GetAwaiter().GetResult();

        Console.ReadKey();

        Console.WriteLine("Cleaning up...");
        projectionStreamS.Dispose();
        projectionSubscription.Dispose();

        httpClient.Dispose();
    }

    public static async Task MainAsync(string[] args, Subject<EqualizerState> projectionSubscription, IConfiguration configuration)
    {
        Console.WriteLine($"Hello {typeof(Program).Namespace}!");

        var client = EventStoreUtil.GetDefaultClient(configuration.GetConnectionString("EVENTSTORE")!);

        var sub = await client.SubscribeToStreamAsync(
            streamName: EventStoreConfiguration.DeviceStreamName,
            start: FromStream.Start,
            eventAppeared: async (subscription, e, cancellationToken) =>
            {
                switch (e.Event.EventType)
                {
                    case nameof(Events.ChannelLevelChanged):
                        await HandleChannelLevelChanged(e, projectionSubscription);
                        break;
                    case nameof(Commands.SetVolume):
                        await HandleSetVolume(e, projectionSubscription);
                        break;
                    default:
                        Console.WriteLine($"Unknown event type {e.Event.EventType}");
                        break;
                }
            }, subscriptionDropped: (subscription, reason, arg3) =>
            {
                Console.WriteLine($"Subscription {subscription.SubscriptionId} dropped {reason} {arg3}");
            });

        //await client.SubscribeToStreamAsync(EventStoreConfiguration.DeviceStreamName, (subscription, e, cancellationToken) =>
        //    {
        //        switch (e.Event.EventType)
        //        {
        //            case nameof(Events.ChannelLevelChanged):
        //                HandleChannelLevelChanged(e, projectionSubscription);
        //                break;
        //            case nameof(Commands.SetVolume):
        //                HandleSetVolume(e, projectionSubscription);
        //                break;
        //            default:
        //                Console.WriteLine($"Unknown event type {e.Event.EventType}");
        //                break;
        //        }

        //        return Task.CompletedTask;
        //    });

        /*
        await client.SubscribeToStreamAsync(EventStoreConfiguration.DeviceStreamName,
            async (subscription, e, cancellationToken) =>
            {
                switch (e.Event.EventType)
                {
                    case nameof(Events.ChannelLevelChanged):
                        await HandleChannelLevelChanged(e, projectionSubscription);
                        break;
                    case nameof(Commands.SetVolume):
                        await HandleSetVolume(e, projectionSubscription);
                        break;
                    default:
                        Console.WriteLine($"Unknown event type {e.Event.EventType}");
                        break;
                }
            });
        */
        /*
        await client.SubscribeToAllAsync(Position.Start,
            (s, e, c) =>
            {
                Console.WriteLine($"OriginalStreamId: {e.OriginalStreamId}");
                Console.WriteLine($"e.Event.EventType: {e.Event.EventType}");
                Console.WriteLine($"e.Event.EventNumber: {e.Event.EventNumber}");
                Console.WriteLine($"e.Event.EventStreamId: {e.Event.EventStreamId}");
                Console.WriteLine($"{e.Event.EventType} @ {e.Event.Position.PreparePosition}");
                return Task.CompletedTask;
            },
            filterOptions: new SubscriptionFilterOptions(EventTypeFilter.ExcludeSystemEvents())
        );
        */
    }

    private static Dictionary<string, Action<EqualizerState, TEvent>> RegisterHandler<TEvent>(Action<EqualizerState, TEvent> modifier)
    {
        var result = new Dictionary<string, Action<EqualizerState, TEvent>>
        {
            [typeof(TEvent).Name] = modifier
        };

        return result;
    }

    private static async Task HandleChannelLevelChanged(ResolvedEvent resolvedEvent, Subject<EqualizerState> projectionSubscription)
    {
        await TransformState<Events.ChannelLevelChanged>(
            resolvedEvent: resolvedEvent,
            projectionSubscription: projectionSubscription,
            modifier: (state, setVolume) =>
            {
                state.Channels
                    = state
                        .Channels
                        .Where(c => c.Channel != setVolume.Channel)
                        .Append(new EqualizerState.EqualizerBandState
                        {
                            Channel = setVolume.Channel,
                            Level = setVolume.Level.ToString().PadLeft(2, '0')
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
}