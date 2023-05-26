namespace ES.Labs.Domain.Projections;

/// <summary>
/// A unit that maintains data integrity and constraints
/// </summary>
public class EqualizerAggregate
{
    private readonly EqualizerState _state = new EqualizerState();

    public EqualizerAggregate()
    {

    }
    
    public void SetVolume(int volume)
    {
        // Check current volume and if the new volume is OK and which event this triggers,
        // ie decrease or increase

        // Current: 75, new: 25 

        if (volume < _state.Volume)
            Apply(new Events.VolumeDecreased(_state.DeviceName, _state.Volume - volume));

        if (volume > _state.Volume)
            Apply(new Events.VolumeIncreased(_state.DeviceName, volume - _state.Volume));
    }

    public async Task Hydrate()
    {

    }

    public async Task EmitEvent(object evt)
    {

    }

    private void Apply(Events.VolumeDecreased volumeDecreased)
    {
        _state.Volume -= volumeDecreased.Decrement;
    }

    private void Apply(Events.VolumeIncreased volumeIncreased)
    {
        _state.Volume -= volumeIncreased.Increment;
    }
}