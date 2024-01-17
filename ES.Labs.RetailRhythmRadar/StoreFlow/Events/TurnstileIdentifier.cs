namespace ES.Labs.RetailRhythmRadar.StoreFlow.Events;

public class TurnstileIdentifier
{
    /// <summary>
    /// An identifier for the turnstile.
    /// Format is for simplicity a store identifier and a turnstile number.
    /// For example "1-1" is the first turnstile in store 1, "1-2" is the second turnstile in store 1.
    /// </summary>
    public required string SerialNumber { get; set; }

    public string GetStore() => SerialNumber.Split('-').FirstOrDefault() ?? "1";

    public string GetLocation() => SerialNumber.Split('-').LastOrDefault() ?? "1";
}