//using ES.Labs.RetailRhythmRadar.StoreFlow.Commands;

//namespace ES.Labs.RetailRhythmRadar.StoreFlow;

//public class StoreZoneProjection
//{
//    public StoreZoneProjection WithZone(string zone)
//    {
//        State = State with { Zone = zone };
//        return this;
//    }

//    public StoreZoneProjection WithRevision(ulong revision)
//    {
//        Revision = revision;
//        return this;
//    }

//    public ulong? Revision { get; set; }

//    public StoreZoneState State { get; set; } = new StoreZoneState("zone", 0, 0, 0);

//    public StoreZoneProjection ApplyEvent(object eventData)
//    {
//        switch (eventData)
//        {
//            case EnterZoneAccepted enterZoneAccepted:
//                State = State with
//                {
//                    CurrentCount = State.CurrentCount + 1,
//                    Max = Math.Max(State.CurrentCount + 1, State.Max),
//                    Zone = enterZoneAccepted.Zone
//                };
//                break;

//            case LeaveZoneRegistered leaveZoneRegistered:
//                State = State with
//                {
//                    CurrentCount = State.CurrentCount - 1,
//                    Min = Math.Min(State.CurrentCount - 1, State.Min),
//                    Zone = leaveZoneRegistered.Zone
//                };
//                break;

//            default:
//                Console.WriteLine($"No handler defined for {eventData.GetType()}");
//                break;
//        }

//        return this;
//    }
//}