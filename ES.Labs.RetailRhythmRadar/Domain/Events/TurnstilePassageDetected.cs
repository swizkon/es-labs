﻿using System.Text.Json.Serialization;

namespace RetailRhythmRadar.Domain.Events;

public class TurnstilePassageDetected
{
    public required TurnstileIdentifier Turnstile { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required TurnstileDirection Direction { get; set; } = TurnstileDirection.Clockwise;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}