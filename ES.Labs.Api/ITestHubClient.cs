using ES.Labs.Domain.Projections;

namespace ES.Labs.Api;

public interface ITestHubClient
{
    Task Notification(string message);

    Task Broadcast(string user, string message);

    Task ChannelLevel(string deviceName, string channel, int value);

    Task VolumeChanged(string deviceName, int volume);

    Task EqualizerStateChanged(EqualizerState state);

    Task Subscribe(string streamName);

    Task Unsubscribe(string streamName);
}