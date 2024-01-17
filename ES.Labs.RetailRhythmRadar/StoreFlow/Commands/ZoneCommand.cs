namespace RetailRhythmRadar.StoreFlow.Commands;

public abstract class ZoneCommand
{
    public required string Store { get; set; }
    public required string Zone { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow; // GetRandomTimestamp();

    private static DateTime GetRandomTimestamp()
    {
        var rnd = Random.Shared;

        var date = DateTime.UtcNow.AddDays(-rnd.Next(7)).Date;

        return date
            .AddHours(rnd.Next(7, 23))
            .AddMinutes(rnd.Next(0, 60));
    }
}