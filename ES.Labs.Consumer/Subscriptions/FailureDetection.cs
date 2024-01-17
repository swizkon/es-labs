using System.Collections.Concurrent;
using System.Net.Mail;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ES.Labs.Domain;
using EventSourcing.EventStoreDB;
using EventStore.Client;
using Microsoft.Extensions.Configuration;
using RetailRhythmRadar.Domain.Events;

namespace ES.Labs.Consumer;

public class FailureDetection
{
    private static readonly ConcurrentDictionary<string, FailureDetectionState> FailureDetectionStates = new();

    public static void StartSubscription(IConfiguration configuration)
    {
        Console.WriteLine("FailureDetection StartSubscription");
        using var projectionSubscription = new Subject<FailureDetectionState>();
        var projectionStreamS = projectionSubscription
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Subscribe(state =>
            {
                Console.WriteLine("Send a message about this");
                try
                {
                    using var server = new SmtpClient(configuration["SmtpHost"] ?? "localhost", int.Parse(configuration["SmtpPort"] ?? "1025"));
                    server.Send(new MailMessage("system@example.com", "support@example.com", $"Trouble with {state.StoreAndZone}", "body"));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

        MainAsync(projectionSubscription, configuration).GetAwaiter().GetResult();

        Console.ReadKey();
        projectionStreamS.Dispose();
        projectionSubscription.Dispose();
    }

    public static async Task MainAsync(Subject<FailureDetectionState> projectionSubscription, IConfiguration configuration)
    {
        Console.WriteLine($"Hello {typeof(Program).Namespace}!");

        var client = EventStoreDbUtils.GetDefaultClient(configuration.GetConnectionString("EVENTSTORE")!);

        var sub = await client.SubscribeToStreamAsync(
            streamName: "$et-zonemanuallyclearedevent",
            start: FromStream.Start,
            resolveLinkTos: true,
            eventAppeared: async (subscription, e, cancellationToken) =>
            {
                switch (e.Event.EventType)
                {
                    case nameof(ZoneManuallyClearedEvent):
                    case "zonemanuallyclearedevent":
                        await HandleZoneManuallyCleared(e, projectionSubscription);
                        break;

                    default:

                        await HandleZoneManuallyCleared(e, projectionSubscription);
                        Console.WriteLine($"Unknown event type {e.Event.EventType}");
                        Console.WriteLine($"Event.EventStreamId {e.Event.EventStreamId}");

                        Console.WriteLine($"IsResolved {e.IsResolved}");
                        Console.WriteLine($"OriginalEvent {e.OriginalEvent.EventType}");
                        Console.WriteLine($"Link?.EventType: {e.Link?.EventType}");
                        Console.WriteLine($"OriginalStreamId: {e.OriginalStreamId}");
                        Console.WriteLine($"OriginalEvent.EventStreamId: {e.OriginalEvent.EventStreamId}");
                        break;
                }
            }, subscriptionDropped: (subscription, reason, arg3) =>
            {
                Console.WriteLine($"Subscription {subscription.SubscriptionId} dropped {reason} {arg3}");
            });
    }

    private static async Task HandleZoneManuallyCleared(ResolvedEvent resolvedEvent, IObserver<FailureDetectionState> projectionSubscription)
    {

        await TransformState<ZoneManuallyClearedEvent>(
            resolvedEvent: resolvedEvent,
            projectionSubscription: projectionSubscription,
            modifier: (state, evt) =>
            {
                Console.WriteLine($"{evt.GetType().Name} {evt.Reason}");

                state.Failures = state.Failures
                    .Append(evt.Timestamp)
                    .Where(f => f > DateTime.UtcNow.AddSeconds(-10))
                    // .TakeLast(5)
                    .ToList();
                state.TotalFailures += 1;
                state.StoreAndZone = $"Store: {evt.Store}, Zone: {evt.Zone}";
            });
    }

    private static async Task TransformState<TEvent>(
        ResolvedEvent resolvedEvent,
        IObserver<FailureDetectionState> projectionSubscription,
        Action<FailureDetectionState, TEvent> modifier)
    {
        var s = FailureDetectionStates.AddOrUpdate(EventStoreConfiguration.DeviceStreamName,
            new FailureDetectionState()
            {
                StoreAndZone = resolvedEvent.OriginalStreamId,
                Failures = new List<DateTime>(),
                TotalFailures = 0
            },
            (s, state) =>
            {
                state.TotalFailures += 1;
                modifier(state, EventStoreDbUtils.GetRecordedEventAs<TEvent>(resolvedEvent.Event));
                return state;
            });

        var moreThanFiveFailureWithinTenSeconds = s.Failures.Count(f => f > DateTime.UtcNow.AddSeconds(-10)) > 5;

        var moreThanOneFailureWithinTwoSeconds = s.Failures.Count(f => f > DateTime.UtcNow.AddSeconds(-2)) > 1;
        // Check if we should emit an error...
        if (moreThanFiveFailureWithinTenSeconds || moreThanOneFailureWithinTwoSeconds)
        {
            Console.WriteLine($"Emitting an error! moreThanFiveFailureWithinTenSeconds: {moreThanFiveFailureWithinTenSeconds} moreThanOneFailureWithinTwoSeconds: {moreThanOneFailureWithinTwoSeconds}");
            projectionSubscription.OnNext(s);
        }
    }

    public static void StopSubscription()
    {
        //projectionStreamS.Dispose();
        //projectionSubscription.Dispose();
    }
}