using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using ES.Labs.Domain;
using ES.Labs.Domain.Projections;
using EventStore.Client;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;

namespace ES.Labs.Consumer;

public class Program
{
    private static readonly ConcurrentDictionary<string, EqualizerState> EqStates = new();

    public static void Main(string[] args)
    {
        var httpClient = new HttpClient();

        using var projectionStream = new Subject<EqualizerState>();
        var projectionStreamS = projectionStream
            .Throttle(TimeSpan.FromMilliseconds(300))
            .Subscribe(state =>
            {
                Console.WriteLine("EMIT state " + state);
                // Send to the api...
                try
                {
                    var d = new StringContent(JsonConvert.SerializeObject(state), Encoding.UTF8, "application/json");
                    var res = httpClient.PostAsync("https://localhost:6001/equalizer/projections", d).Result;
                    Console.WriteLine(res.StatusCode);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

        MainAsync(args, projectionStream).GetAwaiter().GetResult();
        Console.ReadKey();
        
        Console.WriteLine("Cleaning up...");
        projectionStreamS.Dispose();
        projectionStream.Dispose();

        httpClient.Dispose();
    }

    public static async Task MainAsync(string[] args, Subject<EqualizerState> projectionStream)
    {
        Console.WriteLine($"Hello {typeof(Program).Namespace}!");

        var client = EventStoreUtil.GetDefaultClient();
        await client.SubscribeToStreamAsync(EventStoreConfiguration.DeviceStreamName,
            async (subscription, e, cancellationToken) => {
                
                // Console.WriteLine($"SubscriptionId {subscription.SubscriptionId}");

                if (e.Event.EventType == "ChannelLevelChanged")
                {
                    await HandleChannelLevelChanged(e, projectionStream);
                }
                else if (e.Event.EventType == "SetVolume")
                {
                    await HandleSetVolume(e, projectionStream);
                }
                else
                {
                    Console.WriteLine(e.Event.EventType);
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

        //var events = client.ReadStreamAsync(
        //    Direction.Forwards,
        //    EventStoreConfiguration.StreamName,
        //    StreamPosition.Start);

        //await foreach (var @event in events)
        //{
        //    Console.WriteLine(@event.Event.EventNumber);
        //    Console.WriteLine(@event.OriginalStreamId);
        //    Console.WriteLine(Encoding.UTF8.GetString(@event.Event.Data.ToArray()));
        //}

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

    private static async Task HandleChannelLevelChanged(ResolvedEvent resolvedEvent, Subject<EqualizerState> projectionStream)
    {

        await TransformState<Events.ChannelLevelChanged>(
            deviceName: "mainroom",
            resolvedEvent: resolvedEvent,
            projectionStream: projectionStream,
            modifier: (state, setVolume) =>
            {
                state.Channels
                    = state
                        .Channels
                        .Where(c => c.Channel != setVolume.Channel)
                        .Append(new EqualizerState.EqualizerChannelState
                        {
                            Channel = setVolume.Channel,
                            Level = setVolume.Level.ToString().PadLeft(3, '0')
                        })
                        .ToList();
            });

        //var (deviceName, channel, level) = EventStoreUtil.GetRecordedEventAs<Events.ChannelLevelChanged>(resolvedEvent);
        
        //var s = EqStates.AddOrUpdate(deviceName,
        //    new EqualizerState
        //    {
        //        DeviceName = deviceName
        //    },
        //    (s, state) =>
        //    {
        //        state.Version += 1;
        //        state.Channels
        //            = state
        //                .Channels
        //                .Where(c => c.Channel != channel)
        //                .Append(new EqualizerState.EqualizerChannelState
        //                {
        //                    Channel = channel,
        //                    Level = level.ToString().PadLeft(3,'0')
        //                })
        //                .ToList();
        //        return state;
        //    });
        
        //projectionStream.OnNext(s);
    }

    private static async Task HandleSetVolume(ResolvedEvent resolvedEvent, Subject<EqualizerState> projectionStream)
    {
        await TransformState<Commands.SetVolume>(
            deviceName: "mainroom",
            resolvedEvent: resolvedEvent,
            projectionStream: projectionStream,
            modifier: (state, setVolume) =>
            {
                state.Volume = setVolume.Volume;
            });
        /*
        var (deviceName, volume) = EventStoreUtil.GetRecordedEventAs<Commands.SetVolume>(resolvedEvent);

        var s = EqStates.AddOrUpdate(deviceName,
            new EqualizerState
            {
                DeviceName = deviceName
            },
            (s, state) =>
            {
                state.Version += 1;
                state.Volume = volume;
                return state;
            });
        
        projectionStream.OnNext(s);
        */
    }


    private static async Task TransformState<TEvent>(string deviceName, ResolvedEvent resolvedEvent, Subject<EqualizerState> projectionStream, Action<EqualizerState, TEvent> modifier)
    {
        var s = EqStates.AddOrUpdate(deviceName,
            new EqualizerState
            {
                DeviceName = deviceName
            },
            (s, state) =>
            {
                state.Version += 1;
                modifier(state, EventStoreUtil.GetRecordedEventAs<TEvent>(resolvedEvent));
                // state.Volume = volume;
                return state;
            });

        projectionStream.OnNext(s);
    }
}