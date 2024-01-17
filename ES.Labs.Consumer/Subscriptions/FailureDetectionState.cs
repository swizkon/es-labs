namespace ES.Labs.Consumer;

public class FailureDetectionState
{
    public required string StoreAndZone { get; set; }

    public required List<DateTime> Failures { get; set; }

    public required int TotalFailures { get; set; }
}
