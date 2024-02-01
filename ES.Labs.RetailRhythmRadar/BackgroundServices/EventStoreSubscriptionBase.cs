using EventSourcing.EventStoreDB;

namespace RetailRhythmRadar.BackgroundServices;

public abstract class EventStoreSubscriptionBase(ILogger logger) : BackgroundService
{
    protected async Task<bool> WaitForEventStore(string streamName, IConfiguration configuration, TimeSpan maxWaitingTime, CancellationToken stoppingToken)
    {
        var client = EventStoreDbUtils.GetDefaultClient(configuration.GetConnectionString("EVENTSTORE")!);
        var maxTimestamp = DateTime.UtcNow.Add(maxWaitingTime);
        while (DateTime.UtcNow < maxTimestamp)
        {
            try
            {
                await client.GetStreamMetadataAsync(streamName, cancellationToken: stoppingToken);
                return true;
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
                logger.LogInformation("EventStoreDB not available. Until time is {MaxTimestamp} will wait for 1 sec and retry...", maxTimestamp);
                await Task.Delay(1_000, stoppingToken);
            }
        }

        return false;
    }
}