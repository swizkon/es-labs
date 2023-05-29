using System.Text;
using ES.Labs.Domain;
using EventStore.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace ES.Labs.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class EqualizerController : ControllerBase
    {
        private readonly EventStoreClient _eventStoreClient;
        private readonly EventDataBuilder _eventDataBuilder;
        private readonly IHubContext<TestHub> _hubContext;

        private readonly ILogger<EqualizerController> _logger;

        public EqualizerController(
            EventStoreClient eventStoreClient,
            EventDataBuilder eventDataBuilder,
            IHubContext<TestHub> hubContext,
            ILogger<EqualizerController> logger)
        {
            _eventStoreClient = eventStoreClient;
            _eventDataBuilder = eventDataBuilder;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpPost(Name = "SetChannelLevel")]
        public async Task<IActionResult> Set(Events.ChannelLevelChanged data)
        {
            const string metadata = "{}";

            var eventType = data.GetType().Name.ToLower();
            var eventData = new EventData(
                eventId: Uuid.NewUuid(),
                type: eventType,
                //isJson: true,
                data: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)),
                metadata: _eventDataBuilder.BuildMetadata(data)
            );

            var result = await _eventStoreClient.AppendToStreamAsync(
                streamName: $"device-{data.DeviceName}",
                expectedState: StreamState.Any,
                eventData: new List<EventData>
                {
                    eventData
                });

            return Ok(result);
        }
        
        //[HttpPost("projections")]
        //public async Task<IActionResult> SetProjection(EqualizerState state)
        //{
        //    _projectionState.EqualizerState = state;
        //    _logger.LogInformation("Got projection {State}", state);

        //    return await GetProjection(state.DeviceName);
        //}

        //[HttpGet("projections/{deviceName}")]
        //public async Task<IActionResult> GetProjection(
        //    [FromRoute] string deviceName)
        //{
        //    var result = await Task.FromResult(_projectionState.EqualizerState);
        //    return Ok(result);
        //}
    }
}