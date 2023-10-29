namespace ES.Labs.Domain;

public static class Events
{
    public record DeviceRegistered(string DeviceName);

    public record ChannelLevelChanged(string DeviceName, string Channel, int Level);

    public record VolumeIncreased(string DeviceName, int Increment);

    public record VolumeDecreased(string DeviceName, int Decrement);

    public record EqBandAdjusted(string DeviceName, EqBand Band, int Level);
}