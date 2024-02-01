using RetailRhythmRadar.Domain.Events;

namespace RetailRhythmRadar.Domain.Projections;

public record EnterAndExists(
    string Store,
    int FrontDoorExits,
    int GiftShopExits,
    DateTime Checkpoint,
    DateTime LastReset)
{
    public static EnterAndExists InitialState(string store) => new(store, 0, 0, DateTime.MinValue, DateTime.MinValue);

    public EnterAndExists ApplyEvent(StoreExitedEvent exited)
    {
        if (exited.Door == "FrontDoor")
        {
            return this with
            {
                Store = exited.Store,
                Checkpoint = exited.Timestamp,
                FrontDoorExits = FrontDoorExits + 1
            };
        }

        if (exited.Door == "GiftShop")
        {
            return this with
            {
                Store = exited.Store,
                Checkpoint = exited.Timestamp,
                GiftShopExits = GiftShopExits + 1
            };
        }

        return this with
        {
            Store = exited.Store,
            Checkpoint = exited.Timestamp
        };
    }
}