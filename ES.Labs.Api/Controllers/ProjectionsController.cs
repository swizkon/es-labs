using System.Text;
using ES.Labs.Domain;
using ES.Labs.Domain.Projections;
using EventStore.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace ES.Labs.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class ProjectionsController : ControllerBase
    {
        private readonly ProjectionState _projectionState;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;

        private static readonly EqualizerState State = new EqualizerState()
        {
            DeviceName = EventStoreConfiguration.DeviceStreamName
        };

        private readonly ILogger<EqualizerController> _logger;

        public ProjectionsController(
            ProjectionState projectionState,
            IDistributedCache cache,
            IConfiguration configuration,
            ILogger<EqualizerController> logger)
        {
            _projectionState = projectionState;
            _cache = cache;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("{deviceName}")]
        public async Task<IActionResult> SetProjection([FromRoute] string deviceName, EqualizerState state)
        {
            var cacheData = JsonSerializer.Serialize(state);
            await _cache.SetStringAsync(FormatCacheKey(state.DeviceName), cacheData);

            _logger.LogInformation("Received state {cacheData}", cacheData);

            _projectionState.Date = DateTime.UtcNow;
            _projectionState.EqualizerState = state;

            return await GetProjection(state.DeviceName);
        }

        [HttpGet("{deviceName}")]
        public async Task<IActionResult> GetProjection([FromRoute] string deviceName)
        {
            //var snap = await _cache.GetStringAsync(FormatCacheKey(State.DeviceName));

            var client = EventStoreUtil.GetDefaultClient(_configuration.GetConnectionString("EVENTSTORE")!);
            var events = client.ReadStreamAsync(
                Direction.Forwards,
                EventStoreConfiguration.DeviceStreamName,
                State.CurrentVersion ?? StreamPosition.Start,
                deadline: TimeSpan.FromMinutes(2));

            await foreach (var @event in events)
            {
                //Console.WriteLine(@event.Event.EventNumber);
                //Console.WriteLine(@event.OriginalStreamId);
                // Console.WriteLine(Encoding.UTF8.GetString(@event.Event.Data.ToArray()));

                // _logger.LogInformation("Got projection {OriginalEventNumber}", @event.OriginalEventNumber);
                State.CurrentVersion = @event.OriginalEventNumber;
                State.Volume += 1;
            }
            
            //_projectionState.Date = DateTime.UtcNow;
            //_projectionState.EqualizerState = State;

            var cacheData = JsonSerializer.Serialize(State);
            await _cache.SetStringAsync(FormatCacheKey(State.DeviceName), cacheData);
            return Ok(State);
        }

        private static string FormatCacheKey(string deviceName) => $"device-{deviceName}";
    }
}