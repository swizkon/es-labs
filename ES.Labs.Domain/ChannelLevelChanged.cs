namespace ES.Labs.Domain;

public static class Events
{
    public record ChannelLevelChanged(string DeviceName, string Channel, int Level);
    
    public record VolumeIncreased(string DeviceName, int Increment);
}
