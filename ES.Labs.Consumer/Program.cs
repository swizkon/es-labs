// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using System.Text;
using ES.Labs.Domain;
using ES.Labs.Domain.Events;
using ES.Labs.Domain.Projections;
using EventStore.Client;
using Newtonsoft.Json;

namespace ES.Labs.Consumer;

public class Program
{
    private static ConcurrentDictionary<string, EqualizerState> _eqStates 
        = new ConcurrentDictionary<string, EqualizerState>();

    public static void Main(string[] args)
    {
        var httpClient = new HttpClient();

        MainAsync(args).GetAwaiter().GetResult();
        Console.ReadKey();

        httpClient.Dispose();
    }

    public static async Task MainAsync(string[] args)
    {
        Console.WriteLine($"Hello {typeof(Program).Namespace}!");
        /*
        await client.SubscribeToStreamAsync(EventStoreConfiguration.StreamName,
            async (subscription, e, cancellationToken) => {
                Console.WriteLine($"Received event {e.OriginalEventNumber}@{e.OriginalStreamId}");
                //await HandleEvent(evnt);
                await Task.Delay(10, cancellationToken);
            });
        */

        var client = EventStoreUtil.GetDefaultClient();
        await client.SubscribeToStreamAsync(EventStoreConfiguration.DeviceStreamName,
            async (subscription, e, cancellationToken) => {
                
                // Console.WriteLine($"Received event {e.OriginalEventNumber}@{e.OriginalStreamId}");
                //await HandleEvent(evnt);

                if (e.Event.EventType == "ChannelLevelChanged")
                {
                    await HandleChannelLevelChanged(e);
                }
                else
                {
                    Console.WriteLine(e.Event.EventType);
                }
                // await Task.Delay(1, cancellationToken);
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

    private static async Task HandleChannelLevelChanged(ResolvedEvent resolvedEvent)
    {
        var data = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
        var (deviceName, channel, level) = JsonConvert.DeserializeObject<ChannelLevelChanged>(data);
        
        var s = _eqStates.AddOrUpdate(deviceName,
            new EqualizerState(),
            (s, state) =>
            {
                state.DeviceName = deviceName;
                state.Channels
                    = state
                        .Channels
                        .Where(c => c.Channel != channel)
                        .Append(new EqualizerState.EqualizerChannelState
                        {
                            Channel = channel,
                            Level = level.ToString().PadLeft(3,'0')
                        })
                        .ToList();
                return state;
            });

        Console.WriteLine(s.ToString());
        // await Task.Delay(1);
    }
}