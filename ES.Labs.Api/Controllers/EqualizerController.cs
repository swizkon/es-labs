using System.Diagnostics.CodeAnalysis;
using System.Text;
using ES.Labs.Domain;
using EventStore.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace ES.Labs.Api.Controllers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class EqualizerController(
        EventStoreClient eventStoreClient,
        EventDataBuilder eventDataBuilder,
        ProjectionState projectionState,
        IHubContext<TestHub> hubContext,
        ILogger<EqualizerController> logger)
    : ControllerBase
{
    private readonly IHubContext<TestHub> _hubContext = hubContext;

    private readonly ILogger<EqualizerController> _logger = logger;

    [HttpPost(Name = "SetChannelLevel")]
    public async Task<IActionResult> Set(Events.ChannelLevelChanged data)
    {
        return Ok(await eventStoreClient.AppendToStreamAsync(
            streamName: $"device-{data.DeviceName}",
            expectedState: StreamState.Any,
            eventData: new[]
            {
                new EventData(
                    eventId: Uuid.NewUuid(),
                    type: data.GetType().Name.ToLower(),
                    //isJson: true,
                    data: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)),
                    metadata: eventDataBuilder.BuildMetadata(data)
                )
            }));
    }

    [HttpPost("PushState", Name = "PushState")]
    public async Task<IActionResult> PushState()
    {
        _logger.LogInformation("PushState to group VolumeIncreased");
        await _hubContext.Clients.Group("VolumeIncreased").SendAsync("EqualizerStateChanged", projectionState);

        return Ok(projectionState);
    }

    [HttpPost("PushStoresState")]
    public async Task<IActionResult> PushStoresState([FromQuery] string store, [FromQuery] int currentCount, [FromQuery] int maxCapacity)
    {
        _logger.LogInformation("PushStoresState to group storestates");
        await _hubContext.Clients.Group("storestates").SendAsync("StoreStateChanged", store, currentCount, maxCapacity);

        return Ok(projectionState);
    }

    [HttpPost("PushZoneState")]
    public async Task<IActionResult> PushZoneState([FromQuery] string store, [FromQuery] string zone, [FromQuery] int currentCount, [FromQuery] int maxCapacity)
    {
        var group = $"store-{store}-states";
        _logger.LogInformation("PushZoneState to group {group}", group);
        await _hubContext.Clients.Group(group).SendAsync("ZoneStateChanged", store, zone, currentCount, maxCapacity);

        return Ok(projectionState);
    }
}