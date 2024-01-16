

using System.Text;
using ES.Labs.Domain;
using EventStore.Client;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ES.Labs.Api;

public class TestHub : Hub<ITestHubClient>
{
    private readonly ProjectionState _projectionState;
    private readonly EventDataBuilder _eventDataBuilder;
    private readonly ILogger<TestHub> _logger;
    private readonly EventStoreClient _client;

    public TestHub(
        ProjectionState projectionState,
        EventDataBuilder eventDataBuilder,
        IConfiguration configuration,
    ILogger<TestHub> logger)
    {
        _client = EventStoreUtil.GetDefaultClient(configuration.GetConnectionString("EVENTSTORE")!);

        _projectionState = projectionState;
        _eventDataBuilder = eventDataBuilder;
        _logger = logger;
    }

    public async Task Broadcast(string user, string message)
    {
        await Clients.All.Broadcast(user, message);
    }

    public async Task SetVolume(string deviceName, string value)
    {
        _logger.LogInformation("SetVolume {DeviceName} {Value}", deviceName, value);
        var data = new Commands.SetVolume(DeviceName: deviceName, Volume: int.Parse(value));

        var eventType = data.GetType().Name;
        var eventData = new EventData(
            eventId: Uuid.NewUuid(),
            type: eventType,
            data: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)),
            metadata: _eventDataBuilder.BuildMetadata(data),
            contentType: "application/json"
        );

        var result = await _client.AppendToStreamAsync(
            streamName: EventStoreConfiguration.DeviceStreamName,
            expectedState: StreamState.Any,
            eventData: new List<EventData>
            {
                eventData
            }, options =>
            {
                options.TimeoutAfter = TimeSpan.FromSeconds(30);
            });

        _logger.LogInformation("Result from the EventStore: {LogPosition} {NextExpectedStreamRevision}", result.LogPosition, result.NextExpectedStreamRevision);
        
        await Clients.All.VolumeChanged(deviceName, int.Parse(value));
    }

    public async Task SetChannelLevel(string deviceName, string channel, string value)
    {
        var data = new Events.ChannelLevelChanged(DeviceName: deviceName, Channel: channel, Level: int.Parse(value));
        
        var eventType = data.GetType().Name;
        var eventData = new EventData(
            eventId: Uuid.NewUuid(),
            type: eventType,
            data: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)),
            metadata: _eventDataBuilder.BuildMetadata(data),
            contentType: "application/json"
        );

        var result = await _client.AppendToStreamAsync(
            streamName: EventStoreConfiguration.DeviceStreamName,
            expectedState: StreamState.Any,
            eventData: new List<EventData>
            {
                eventData
            });

        // _logger.LogInformation("Result from the EventStore: {LogPosition} {NextExpectedStreamRevision}", result.LogPosition, result.NextExpectedStreamRevision);

        await Clients.All.ChannelLevel(deviceName, channel, int.Parse(value));
    }

    public async Task Subscribe(string streamName)
    {
        _logger.LogInformation("Subscribe to {streamName}", streamName);
        await Groups.AddToGroupAsync(Context.ConnectionId, streamName);
        await Clients.Group(streamName).Notification($"{Context.ConnectionId} Subscribed to {streamName}");
    }

    public async Task Unsubscribe(string streamName)
    {
        _logger.LogInformation("Unsubscribe from {streamName}", streamName);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, streamName);

        await Clients.Group(streamName).Notification($"{Context.ConnectionId} Unsubscribe from {streamName}");
    }
    
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("OnConnectedAsync");
        if (_projectionState.EqualizerState != null)
        {
            _logger.LogInformation("Send state");
            await Clients.All.EqualizerStateChanged(_projectionState.EqualizerState);
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, "All Connected Users");
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine(exception?.Message);
        return base.OnDisconnectedAsync(exception);
    }
}
