using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using ES.Labs.Domain;
using ES.Labs.Domain.Projections;
using EventStore.Client;
using Newtonsoft.Json;

namespace ES.Labs.Consumer;

public class Program
{
    private static readonly ConcurrentDictionary<string, EqualizerState> EqStates = new();

    public static void Main(string[] args)
    {
        var httpClient = new HttpClient();

        using var projectionSubscription = new Subject<EqualizerState>();
        var projectionStreamS = projectionSubscription
            .Throttle(TimeSpan.FromMilliseconds(1000))
            .Subscribe(state =>
            {
                Console.WriteLine("EMIT state " + state);
                // Send to the api...
                try
                {
                    var d = new StringContent(JsonConvert.SerializeObject(state), Encoding.UTF8, "application/json");
                    
                    var res = httpClient.PostAsync($"https://localhost:6001/projections/{state.DeviceName}", d).Result;
                    Console.WriteLine(res.StatusCode);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

        MainAsync(args, projectionSubscription).GetAwaiter().GetResult();
        Console.ReadKey();

        Console.WriteLine("Cleaning up...");
        projectionStreamS.Dispose();
        projectionSubscription.Dispose();

        httpClient.Dispose();
    }

    public static async Task MainAsync(string[] args, Subject<EqualizerState> projectionSubscription)
    {
        Console.WriteLine($"Hello {typeof(Program).Namespace}!");

        var client = EventStoreUtil.GetDefaultClient();

        //var dd = new EqualizerAggregate(client,new EventDataBuilder());
        //await dd.Hydrate();

        // return;  

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
            });

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

        //var endTime = DateTime.UtcNow.AddMinutes(2);
        //var position = Position.Start;

        //while (DateTime.UtcNow < endTime)
        //{
        //    var allEvents = client.ReadAllAsync(Direction.Forwards, position);
        //    await foreach (var e in allEvents)
        //    {
        //        position = e.OriginalPosition ?? e.Event.Position;
        //        if (e.Event.EventType.StartsWith("$"))
        //        {
        //            continue;
        //        }

        //        Console.WriteLine($"OriginalStreamId: {e.OriginalStreamId}");
        //        Console.WriteLine($"e.Event.EventType: {e.Event.EventType}");
        //        Console.WriteLine($"e.Event.EventNumber: {e.Event.EventNumber}");
        //        Console.WriteLine($"e.Event.EventStreamId: {e.Event.EventStreamId}");

        //        // Console.WriteLine(e.Event.EventType);

        //        if (e.OriginalStreamId != EventStoreConfiguration.StreamName)
        //            continue;

        //        Console.WriteLine(Encoding.UTF8.GetString(e.Event.Data.ToArray()));
        //    }

        //    Console.WriteLine("Wait for new round...");
        //    await Task.Delay(10_000);
        //}
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