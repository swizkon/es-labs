using System.Text;
using ES.Labs.Domain.Events;
using ES.Labs.Domain.Projections;
using EventStore.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
    
namespace ES.Labs.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EqualizerController : ControllerBase
    {
        private readonly EventStoreClient _eventStoreClient;
        private readonly IHubContext<TestHub> _hubContext;

        private readonly ILogger<EqualizerController> _logger;

        public EqualizerController(
            EventStoreClient eventStoreClient,
            IHubContext<TestHub> hubContext, 
            ILogger<EqualizerController> logger)
        {
            _eventStoreClient = eventStoreClient;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpGet(Name = "GetEqualizer")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55)
            })
                .ToArray();
        }

        [HttpPost(Name = "SetChannelLevel")]
        public async Task<IActionResult> Set(ChannelLevelChanged data)
        {
            const string metadata = "{}";

            var eventType = data.GetType().Name.ToLower();
            var eventData = new EventData(
                eventId: Uuid.NewUuid(),
                type: eventType,
                //isJson: true,
                data: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)),
                metadata: Encoding.UTF8.GetBytes(metadata)
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

        //[HttpPost(template: "channel", Name = "AdjustEqChannel")]
        //public async Task<IActionResult> AdjustEqChannel(ChannelLevelChanged data)
        //{
        //    const string metadata = "{}";

        //    var eventType = data.GetType().Name.ToLower();
        //    var eventData = new EventData(
        //        eventId: Uuid.NewUuid(),
        //        type: eventType,
        //        //isJson: true,
        //        data: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)),
        //        metadata: Encoding.UTF8.GetBytes(metadata)
        //    );

        //    var result = await _eventStoreClient.AppendToStreamAsync(
        //        streamName: $"device-{data.DeviceName}",
        //        expectedState: StreamState.Any,
        //        eventData: new List<EventData>
        //        {
        //            eventData
        //        });

        //    return Ok(result);
        //}

        [HttpPost("projections")]
        public async Task<IActionResult> SetProjection(EqualizerState state)
        {
            _logger.LogInformation("Got projection {State}", state);

            return Ok(state);
        }

        [HttpGet("projections/{deviceName}")]
        public async Task<IActionResult> GetProjection(
            [FromRoute] string deviceName)
        {
            return Ok(deviceName);
        }
    }
}