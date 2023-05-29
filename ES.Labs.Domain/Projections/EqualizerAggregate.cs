using System.Diagnostics;
using System.Reflection;
using System.Text;
using EventStore.Client;
using Newtonsoft.Json;

namespace ES.Labs.Domain.Projections;

/// <summary>
/// A unit that maintains data integrity, invariants and constraints
/// </summary>
public class EqualizerAggregate 
{
    private readonly EventStoreClient _client;
    private readonly EqualizerState _state = new EqualizerState();
    
    public StreamPosition? CurrentStreamPosition { get; set; }

    public EqualizerAggregate(EventStoreClient client) => _client = client;

    public async Task Hydrate()
    {
        var timer = Stopwatch.StartNew();
        Console.WriteLine("Begin hydrate");
        Console.WriteLine($"{timer.Elapsed} Start read");
        var allEvents = _client.ReadStreamAsync(
            direction: Direction.Forwards,
            streamName: EventStoreConfiguration.DeviceStreamName,
            revision: CurrentStreamPosition ?? StreamPosition.Start,
            configureOperationOptions: options =>
            {
                options.TimeoutAfter = TimeSpan.FromHours(1);
            });

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

    private async Task AcceptChange(object evt)
    {

    }

    private async Task ApplyEvent(EventRecord evt)
    {
        var metadata = JsonConvert.DeserializeObject<IDictionary<string,string>>(
            Encoding.UTF8.GetString(evt.Metadata.ToArray()));
        
        var eventType = Type.GetType(metadata["CtrlType"])!;

        var parsedEvent = EventStoreUtil.GetRecordedEvent(evt, eventType);

        var methods = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);

        var matchingmethods = methods
            .FirstOrDefault(m => m.GetParameters().FirstOrDefault()?.ParameterType == eventType);
        try
        {
            matchingmethods?.Invoke(this, new []{parsedEvent} );
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

    private void Apply(Events.VolumeIncreased volumeIncreased)
    {
        _state.Volume -= volumeIncreased.Increment;
    }

    private void Apply(Commands.SetVolume volume)
    {
        Console.WriteLine("Set volume to " + volume.Volume);
        _state.Volume = volume.Volume;
    }
}