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

        [HttpGet("{deviceName}")]
        public async Task<IActionResult> GetProjection([FromRoute] string deviceName)
        {
            var snap = await _cache.GetStringAsync($"device-{deviceName}");

            var client = EventStoreUtil.GetDefaultClient();
            var events = client.ReadStreamAsync(
                Direction.Forwards,
                EventStoreConfiguration.DeviceStreamName,
                State.Version + 1,
                // deadline: TimeSpan.FromMinutes(2));

                configureOperationOptions: options =>
                {
                    options.TimeoutAfter = TimeSpan.FromMinutes(2);
                });

            await foreach (var @event in events)
            {
                //Console.WriteLine(@event.Event.EventNumber);
                //Console.WriteLine(@event.OriginalStreamId);
                //Console.WriteLine(Encoding.UTF8.GetString(@event.Event.Data.ToArray()));

                State.Version = @event.OriginalEventNumber;
                State.Volume += 1;
            }

            return Ok(State);
        }
    }
}