using Microsoft.AspNetCore.SignalR;
using EventSourcing;
using Microsoft.Extensions.Caching.Distributed;
using RetailRhythmRadar.Domain.Events;
using RetailRhythmRadar.Domain.Projections;

namespace RetailRhythmRadar.Hubs;

public class MessageExchangeHub : Hub<IMessageExchangeHubClient>
{
    //private readonly ProjectionState _projectionState;
    //private readonly EventDataBuilder _eventDataBuilder;
    private readonly IWriteEvents _eventWriter;
    private readonly IReadStreams _eventReader;

    private readonly ILogger<MessageExchangeHub> _logger;

    private IDistributedCache _cache;
    //private readonly EventStoreClient _client;

    public MessageExchangeHub(
        // ProjectionState projectionState,
        // EventDataBuilder eventDataBuilder,
        IWriteEvents eventWriter,
        IReadStreams eventReader,
        IDistributedCache cache,
        ILogger<MessageExchangeHub> logger)
    {
        _eventWriter = eventWriter;
        _eventReader = eventReader;
        _cache = cache;
        _logger = logger;
    }

    public async Task Notification(string message)
    {
        _logger.LogInformation("Notification {Message}", message);
        await Clients.All.Notification(message + " YEAH");
    }

    public async Task SetZoneThreshold(string zone, string threshold)
    {
        var thresholdValue = int.Parse(threshold);

        var evt = new ZoneThresholdConfiguredEvent
        {
            Zone = zone,
            Threshold = thresholdValue,
            Timestamp = DateTime.UtcNow
        };

        await _eventWriter.WriteEventAsync("configs", evt);
        // await Clients.Group("configs").ZoneThresholdChanged(evt.Zone, evt.Threshold);
        var state = await new ConfigProjection(_eventReader, _cache).BuildAsync(CancellationToken.None);

        var zoneThresholds = state.ZoneVisitor.Select(x => new
        {
            zone = x.Key,
            threshold = x.Value
        });
        await Clients.Group("configs").ConfigChanged(zoneThresholds);
    }

    // 
    // await _hubConnection.SendAsync("PushStoresState", message.Store, state.ZoneVisitor.Sum(x => x.Value), 50, context.CancellationToken);
    public async Task PushStoresState(string store, string currentCount, string maxCapacity)
    {
        _logger.LogInformation("PushStoresState {store}", store);
        // await Clients.All.Notification(message);
    }

    //public async Task SetVolume(string deviceName, string value)
    //{
    //    _logger.LogInformation("SetVolume {DeviceName} {Value}", deviceName, value);
    //    var data = new Commands.SetVolume(DeviceName: deviceName, Volume: int.Parse(value));

    //    var eventType = data.GetType().Name;
    //    var eventData = new EventData(
    //        eventId: Uuid.NewUuid(),
    //        type: eventType,
    //        data: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)),
    //        metadata: _eventDataBuilder.BuildMetadata(data),
    //        contentType: "application/json"
    //    );

    //    var result = await _client.AppendToStreamAsync(
    //        streamName: EventStoreConfiguration.DeviceStreamName,
    //        expectedState: StreamState.Any,
    //        eventData: new List<EventData>
    //        {
    //            eventData
    //        }, options =>
    //        {
    //            options.TimeoutAfter = TimeSpan.FromSeconds(30);
    //        });

    //    _logger.LogInformation("Result from the EventStore: {LogPosition} {NextExpectedStreamRevision}", result.LogPosition, result.NextExpectedStreamRevision);

    //    await Clients.All.VolumeChanged(deviceName, int.Parse(value));
    //}

    //public async Task SetChannelLevel(string deviceName, string channel, string value)
    //{
    //    var data = new Events.ChannelLevelChanged(DeviceName: deviceName, Channel: channel, Level: int.Parse(value));

    //    var eventType = data.GetType().Name;
    //    var eventData = new EventData(
    //        eventId: Uuid.NewUuid(),
    //        type: eventType,
    //        data: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)),
    //        metadata: _eventDataBuilder.BuildMetadata(data),
    //        contentType: "application/json"
    //    );

    //    var result = await _client.AppendToStreamAsync(
    //        streamName: EventStoreConfiguration.DeviceStreamName,
    //        expectedState: StreamState.Any,
    //        eventData: new List<EventData>
    //        {
    //            eventData
    //        });

    //    // _logger.LogInformation("Result from the EventStore: {LogPosition} {NextExpectedStreamRevision}", result.LogPosition, result.NextExpectedStreamRevision);

    //    await Clients.All.ChannelLevel(deviceName, channel, int.Parse(value));
    //}

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
        _logger.LogInformation("OnConnectedAsync to {streamName}", Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, "All Connected Users");
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("OnDisconnectedAsync to {streamName}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
