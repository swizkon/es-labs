namespace ES.Labs.Domain.Projections;

public class EqualizerState
{
    public ulong? CurrentVersion { get; set; }

    public string? DeviceName { get; set; }

    public int Volume { get; set; }

    public List<EqualizerBandState> Channels { get; set; } = new List<EqualizerBandState>()
    {
        new() { Channel = "0", Level = "0" },
    };

    public class EqualizerBandState
    {
        public string? Channel { get; set; }
        public string? Level { get; set; }
    }

    public override string ToString()
    {
        return $"{CurrentVersion} {DeviceName}: Volume {Volume} " + string.Join(", ", Channels.OrderBy(c => c.Channel).Select(c => $"{c.Channel}={c.Level}"));
    }
}