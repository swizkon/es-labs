using EventSourcing;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using RetailRhythmRadar.Domain.Events;
using RetailRhythmRadar.Domain.Projections;
using System.Net.Mail;

namespace RetailRhythmRadar.Domain.Handlers;

public class TooCrowdedWatcher :
    // IConsumer<StoreStateChanged>
    IConsumer<ZoneEnteredEvent>
{
    private readonly IReadStreams _eventStreams;
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TooCrowdedWatcher> _logger;

    public TooCrowdedWatcher(
        IReadStreams eventStreams,
        IDistributedCache cache,
        IConfiguration configuration,
        ILogger<TooCrowdedWatcher> logger)
    {
        _eventStreams = eventStreams;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ZoneEnteredEvent> context)
    {
        _logger.LogInformation("ZoneEnteredEvent {Store} {Zone} {Timestamp}", context.Message.Store, context.Message.Zone, context.Message.Timestamp);

        var message = context.Message;
        var state = await new SingleStoreProjection(message.Store, message.Timestamp)
            .WithCache(_cache)
            .WithEventDataBuilder(_eventStreams)
            .BuildAsync(context.CancellationToken);

        var numberOfVisitorsInZone = state.ZoneVisitor.FirstOrDefault(x => x.Key == message.Zone).Value;

        if (numberOfVisitorsInZone < 0)
        {
            _logger.LogInformation("Zone {Zone} is empty. Return...", message.Zone);
            return;
        }

        var config = await new ConfigProjection(_eventStreams, _cache).BuildAsync(CancellationToken.None);

        if (config.ZoneVisitor.TryGetValue(message.Zone, out var threshold))
        {
            if (numberOfVisitorsInZone > threshold)
            {
                // Here maybe we want to check some timestamp to enbale the zone to be over threshold for some time
                _logger.LogWarning("Zone {Zone} in store {Store} is over threshold. {NumberOfVisitorsInZone} > {Threshold}", message.Zone, message.Store, numberOfVisitorsInZone, threshold);
            }

            if (numberOfVisitorsInZone > threshold * 2)
            {
                _logger.LogError("Zone {Zone} is too crowded! {NumberOfVisitorsInZone} > {Threshold}", message.Zone, numberOfVisitorsInZone, threshold * 2);

                try
                {
                    using var server = new SmtpClient(_configuration["SmtpHost"] ?? "localhost", int.Parse(_configuration["SmtpPort"] ?? "1025"));
                    server.Send(new MailMessage($"{message.Store}-{message.Zone}@example.com", "support@example.com", $"Very crowded in {message.Store}/{message.Zone}!", "body"));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to send email");
                }
            }
        }
    }
}