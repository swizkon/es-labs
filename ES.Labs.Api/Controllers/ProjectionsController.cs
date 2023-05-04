using ES.Labs.Domain;
using ES.Labs.Domain.Projections;
using EventStore.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace ES.Labs.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class ProjectionsController : ControllerBase
    {
        private readonly IDistributedCache _cache;

        private static readonly EqualizerState State = new EqualizerState();

        private readonly ILogger<EqualizerController> _logger;

        public ProjectionsController(
            IDistributedCache cache,
            ILogger<EqualizerController> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        [HttpPost("{deviceName}")]
        public async Task<IActionResult> SetProjection([FromRoute] string deviceName, EqualizerState state)
        {
            var cacheData = JsonConvert.SerializeObject(state);
            await _cache.SetStringAsync($"device-{state.DeviceName}", cacheData);

            _logger.LogInformation("Got projection {cacheData}", cacheData);

            return await GetProjection(state.DeviceName);
        }

        [HttpGet("{deviceName}")]
        public async Task<IActionResult> GetProjection([FromRoute] string deviceName)
        {
            var snap = await _cache.GetStringAsync($"device-{deviceName}");

            var client = EventStoreUtil.GetDefaultClient();
            var events = client.ReadStreamAsync(
                Direction.Forwards,
                EventStoreConfiguration.DeviceStreamName,
                State.CurrentVersion ?? StreamPosition.Start,
                configureOperationOptions: options =>
                {
                    options.TimeoutAfter = TimeSpan.FromMinutes(2);
                });

            await foreach (var @event in events)
            {
                //Console.WriteLine(@event.Event.EventNumber);
                //Console.WriteLine(@event.OriginalStreamId);
                //Console.WriteLine(Encoding.UTF8.GetString(@event.Event.Data.ToArray()));

                State.CurrentVersion = @event.OriginalEventNumber;
                State.Volume += 1;
            }

            return Ok(State);
        }
    }
}