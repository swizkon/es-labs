namespace RetailRhythmRadar.BackgroundServices;

public static class TimestampExtensions
{
    public static TimeSpan Milliseconds(this int milliseconds) => TimeSpan.FromMilliseconds(milliseconds);
}