namespace ES.Labs.Domain;

public static class Commands
{
    public record SetVolume(string DeviceName, int Volume);

    public record SetChannelLevel(string DeviceName, string Channel, int Level);
}