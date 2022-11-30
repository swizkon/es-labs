namespace ES.Labs.Domain.Projections;

public class EqualizerState
{
    public int Version { get; set; } 
    public string DeviceName { get; set; }

    public int Volume { get; set; }

    public List<EqualizerChannelState> Channels { get; set; } = new List<EqualizerChannelState>();

    public class EqualizerChannelState
    {
        public string Channel { get; set; }
        public string Level { get; set; }
    }

    public override string ToString()
    {
        return $"{Version} {DeviceName}: Volume {Volume} " + string.Join(", ", Channels.OrderBy(c => c.Channel).Select(c => $"{c.Channel}={c.Level}"));
    }
}