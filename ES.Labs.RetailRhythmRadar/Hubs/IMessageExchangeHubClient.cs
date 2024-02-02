
namespace RetailRhythmRadar.Hubs;

public interface IMessageExchangeHubClient
{
    Task Notification(string message);

    Task ChannelLevel(string deviceName, string channel, int value);

    Task ZoneThresholdChanged(string zone, int threshold);  
    
    Task Subscribe(string streamName);

    Task Unsubscribe(string streamName);
}