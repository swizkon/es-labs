using ES.Labs.Domain.Projections;

namespace ES.Labs.Api;

public class ProjectionState
{
    public DateTime Date { get; set; }

    public EqualizerState? EqualizerState { get; set; }
}