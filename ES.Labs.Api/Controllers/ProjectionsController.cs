using System.Text;
using ES.Labs.Domain;
using ES.Labs.Domain.Projections;
using EventStore.Client;
using Microsoft.AspNetCore.Mvc;

namespace ES.Labs.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProjectionsController : ControllerBase
    {
        private readonly EventStoreClient _eventStoreClient;

        private static readonly EqualizerState State = new EqualizerState();

        private readonly ILogger<EqualizerController> _logger;

        public ProjectionsController(
            EventStoreClient eventStoreClient,
            ILogger<EqualizerController> logger)
        {
            _eventStoreClient = eventStoreClient;
            _logger = logger;
        }

        [HttpGet("{deviceName}")]
        public async Task<IActionResult> GetProjection([FromRoute] string deviceName)
        {
            var client = EventStoreUtil.GetDefaultClient();
            var events = client.ReadStreamAsync(
                Direction.Forwards,
                EventStoreConfiguration.DeviceStreamName,
                State.Version);

            await foreach (var @event in events)
            {
                Console.WriteLine(@event.Event.EventNumber);
                Console.WriteLine(@event.OriginalStreamId);
                Console.WriteLine(Encoding.UTF8.GetString(@event.Event.Data.ToArray()));

                State.Version = @event.OriginalEventNumber + 1;
            }

            return Ok(State);
        }
    }
}