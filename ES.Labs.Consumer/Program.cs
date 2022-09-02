// See https://aka.ms/new-console-template for more information

//using System.Diagnostics;
using System.Text;
using ES.Labs.Domain;
using EventStore.Client;

public class Program
{
    public static void Main(string[] args)
    {
        MainAsync(args).GetAwaiter().GetResult();
        Console.ReadKey();
    }

    public static async Task MainAsync(string[] args)
    {
        await Task.Delay(1000);
        Console.WriteLine("Hello World!");
        
        var settings = EventStoreClientSettings
            .Create("esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false");
        var client = new EventStoreClient(settings);

        await client.SubscribeToStreamAsync(EventStoreConfiguration.StreamName,
            async (subscription, e, cancellationToken) => {
                Console.WriteLine($"Received event {e.OriginalEventNumber}@{e.OriginalStreamId}");
                //await HandleEvent(evnt);
                await Task.Delay(10, cancellationToken);
            });

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
}
