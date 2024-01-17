
namespace RetailRhythmRadar.Hubs;

public interface IMessageExchangeHubClient
{
    Task Notification(string message);

    Task ChannelLevel(string deviceName, string channel, int value);

    Task VolumeChanged(string deviceName, int volume);
    
    Task Subscribe(string streamName);

    Task Unsubscribe(string streamName);
}