

using System.Text;
using ES.Labs.Domain;
using ES.Labs.Domain.Events;
using EventStore.Client;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace ES.Labs.Api;

public class TestHub : Hub<ITestHubClient>
{
    private readonly ILogger<TestHub> _logger;
    private readonly EventStoreClient _client;

    public TestHub(ILogger<TestHub> logger)
    {
        var settings = EventStoreClientSettings
            .Create("esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false");
        _client = new EventStoreClient(settings);

        _logger = logger;
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.ReceiveMessage(user, message);
    }

    public async Task Broadcast(string user, string message)
    {
        await Clients.All.Broadcast(user, message);
    }

    public async Task SetChannelLevel(string deviceName, string channel, string value)
    {
        var data = new ChannelLevelChanged(DeviceName: deviceName, Channel: channel, Level: int.Parse(value));
        
        var metadata = new
        {
            Timestamp = DateTime.UtcNow.ToString("o"),
            CtrlType = data.GetType().FullName,
            data.GetType().AssemblyQualifiedName
        };

        var eventType = data.GetType().Name; //.ToLower();
        var eventData = new EventData(
            eventId: Uuid.NewUuid(),
            type: eventType,
            data: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)),
            metadata: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata)),
            contentType: "application/json"
        );

        var result = await _client.AppendToStreamAsync(
            streamName: EventStoreConfiguration.DeviceStreamName,
            expectedState: StreamState.Any,
            eventData: new List<EventData>
            {
                eventData
            });

        _logger.LogInformation("Result from the EventStore: {LogPosition} {NextExpectedStreamRevision}", result.LogPosition, result.NextExpectedStreamRevision);

        await Clients.All.ChannelLevel(deviceName, channel, int.Parse(value));
    }

    public async Task PlayerPosition(string player, int x, int y)
    {
        await Clients.All.PlayerPosition(player, x, y);
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "All Connected Users");
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine(exception?.Message);
        return base.OnDisconnectedAsync(exception);
    }
}
