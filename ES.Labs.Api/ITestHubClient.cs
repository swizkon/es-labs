namespace ES.Labs.Api;

public interface ITestHubClient
{
    Task ReceiveMessage(string user, string message);

    Task Broadcast(string user, string message);

    Task ChannelLevel(string deviceName, string channel, int value);

    Task PlayerPosition(string player, int x, int y);
}