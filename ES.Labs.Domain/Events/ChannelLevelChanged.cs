namespace ES.Labs.Domain.Events;

public record ChannelLevelChanged(string DeviceName, string Channel, int Level);