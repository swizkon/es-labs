using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using EventStore.Client;
using static ES.Labs.Domain.Events;

namespace ES.Labs.Domain.Projections;

/// <summary>
/// A unit that maintains data integrity, invariants and constraints
/// </summary>
public class EqualizerAggregate(
    EventStoreClient client,
    EventDataBuilder eventDataBuilder)
{
    private readonly EqualizerState _state = new(){DeviceName = EventStoreConfiguration.DeviceStreamName };

    public StreamPosition? CurrentStreamPosition { get; set; }

    public async Task InitStream()
    {
        var data = new DeviceRegistered(DeviceName: EventStoreConfiguration.DeviceStreamName);
        await client.AppendToStreamAsync(
            streamName: EventStoreConfiguration.DeviceStreamName,
            expectedState: StreamState.NoStream,
            eventData: new[]
            {
                new EventData(
                    eventId: Uuid.NewUuid(),
                    type: data.GetType().Name.ToLower(),
                //isJson: true,
                    data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data)),
                    metadata: eventDataBuilder.BuildMetadata(data)
                )
            });
    }

    public async Task Hydrate()
    {
        var timer = Stopwatch.StartNew();
        Console.WriteLine("Begin hydrate");
        Console.WriteLine($"{timer.Elapsed} Start read");
        var allEvents = client.ReadStreamAsync(
            direction: Direction.Forwards,
            streamName: EventStoreConfiguration.DeviceStreamName,
            revision: CurrentStreamPosition ?? StreamPosition.Start,
            deadline: TimeSpan.FromHours(1));

            //configureOperationOptions: options =>
            //{
            //    options.TimeoutAfter = TimeSpan.FromHours(1);
            //});

        Console.WriteLine($"{timer.Elapsed} read completed");
        await foreach (var e in allEvents)
        {
            if (e.Event.EventType.StartsWith("$"))
            {
                continue;
            }

            if (e.OriginalStreamId != EventStoreConfiguration.DeviceStreamName)
                continue;

            await ApplyEvent(e.Event);
        }
        Console.WriteLine($"hydrate done in {timer.Elapsed}");
    }

    public void SetVolume(int volume)
    {
        if (volume < _state.Volume)
            Apply(new Events.VolumeDecreased(_state.DeviceName, _state.Volume - volume));

        if (volume > _state.Volume)
            Apply(new Events.VolumeIncreased(_state.DeviceName, volume - _state.Volume));
    }
    
    private async Task ApplyEvent(EventRecord evt)
    {
        var metadata = JsonSerializer.Deserialize<IDictionary<string, string>>(
            Encoding.UTF8.GetString(evt.Metadata.ToArray()))!;

        var eventType = Type.GetType(metadata["CtrlType"])!;

        var parsedEvent = EventStoreUtil.GetRecordedEvent(evt, eventType);

        var methods = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);

        var matchingMethods = methods
            .FirstOrDefault(m => m.GetParameters().FirstOrDefault()?.ParameterType == eventType);
        try
        {
            matchingMethods?.Invoke(this, new[] { parsedEvent });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            // throw;
        }
        CurrentStreamPosition = evt.EventNumber;
    }

    private void Apply(Events.VolumeDecreased volumeDecreased)
    {
        Console.WriteLine("Decrease volume by " + volumeDecreased.Decrement);
        _state.Volume -= volumeDecreased.Decrement;
    }

    private void Apply(Events.VolumeIncreased volumeIncreased) => _state.Volume -= volumeIncreased.Increment;

    private void Apply(Commands.SetVolume volume)
    {
        Console.WriteLine("Set volume to " + volume.Volume);
        _state.Volume = volume.Volume;
    }
}