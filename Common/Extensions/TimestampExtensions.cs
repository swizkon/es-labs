namespace Common.Extensions;

public static class TimestampExtensions
{
    public static TimeSpan Milliseconds(this int milliseconds) => TimeSpan.FromMilliseconds(milliseconds);
}