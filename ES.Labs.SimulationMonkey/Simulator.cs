using RetailRhythmRadar.Domain.Events;
using RetailRhythmRadar.Domain.Projections;

public static class Simulator
{
    public static IEnumerable<TurnstilePassageDetected> GetSimulatedActions(int storeNumber, SingleStoreState singleStoreState, DateTime currentTime)
    {
        return GetActionsForZoneA(storeNumber, singleStoreState, currentTime)
            .Concat(GetActionsForZoneB(storeNumber, singleStoreState, currentTime))
            .Concat(GetActionsForZoneC(storeNumber, singleStoreState, currentTime))
            .Concat(GetActionsForZoneD(storeNumber, singleStoreState, currentTime));
    }


    private static IEnumerable<TurnstilePassageDetected> GetActionsForZoneA(int storeNumber, SingleStoreState singleStoreState, DateTime timestamp)
    {
        if (singleStoreState.ZoneA > 0 && timestamp.Hour > 20)
        {
            yield return ZoneTransition(storeNumber, "A", "B", timestamp);
            yield break;
        }

        if (singleStoreState.ZoneA < Random.Shared.Next(0, 50) - timestamp.Hour && timestamp.Hour < 21)
        {
            yield return StoreEnter(storeNumber, "A", timestamp);
        }

        if (singleStoreState.ZoneA > Random.Shared.Next(0, 100) || timestamp.Hour > 21)
        {
            // Possibility for either exit or zone transition
            var rnd = Random.Shared.Next(0, 100);

            // Exit
            if (rnd < 10)
            {
                yield return StoreExit(storeNumber, "A", timestamp);
            }
            else if (rnd < 45)
            {
                yield return ZoneTransition(storeNumber, "A", "C", timestamp);
            }
            else
            {
                yield return ZoneTransition(storeNumber, "A", "B", timestamp);
            }
        }
    }

    private static IEnumerable<TurnstilePassageDetected> GetActionsForZoneB(int storeNumber, SingleStoreState singleStoreState, DateTime timestamp)
    {
        if (singleStoreState.ZoneB > 0 && timestamp.Hour > 21)
        {
            yield return ZoneTransition(storeNumber, "B", "D", timestamp);
            yield break;
        }

        if (singleStoreState.ZoneB > Random.Shared.Next(0, 100) || timestamp.Hour > 21)
        {
            // Possibility for either exit or zone transition
            var rnd = Random.Shared.Next(0, 100);

            // Exit
            if (rnd < 5 && timestamp.Hour < 21)
            {
                yield return ZoneTransition(storeNumber, "B", "A", timestamp);
            }
            else if (rnd < 45 && timestamp.Hour < 21)
            {
                yield return ZoneTransition(storeNumber, "B", "C", timestamp);
            }
            else
            {
                yield return ZoneTransition(storeNumber, "B", "D", timestamp);
            }
        }
    }

    private static IEnumerable<TurnstilePassageDetected> GetActionsForZoneC(int storeNumber, SingleStoreState singleStoreState, DateTime timestamp)
    {
        if (singleStoreState.ZoneC > 0 && timestamp.Hour > 21)
        {
            yield return ZoneTransition(storeNumber, "C", "D", timestamp);
            yield break;
        }

        if (singleStoreState.ZoneC > Random.Shared.Next(0, 100) || timestamp.Hour > 21)
        {
            // Possibility for either exit or zone transition
            var rnd = Random.Shared.Next(0, 100);

            // Exit
            if (rnd < 5 && timestamp.Hour < 21)
            {
                yield return ZoneTransition(storeNumber, "C", "A", timestamp);
            }
            else if (rnd < 30 && timestamp.Hour < 21)
            {
                yield return ZoneTransition(storeNumber, "C", "B", timestamp);
            }
            else
            {
                yield return ZoneTransition(storeNumber, "C", "D", timestamp);
            }
        }
    }
    
    private static IEnumerable<TurnstilePassageDetected> GetActionsForZoneD(int storeNumber, SingleStoreState singleStoreState, DateTime timestamp)
    {
        if (singleStoreState.ZoneD > Random.Shared.Next(0, 100) || timestamp.Hour > 21)
        {
            yield return StoreExit(storeNumber, "D", timestamp);
            yield break;
        }

        if (singleStoreState.ZoneD > Random.Shared.Next(0, 100) || timestamp.Hour > 21)
        {
            // Possibility for either exit or zone transition
            var rnd = Random.Shared.Next(0, 100);

            // Exit
            if (rnd < 2 && timestamp.Hour < 21)
            {
                yield return StoreEnter(storeNumber, "D", timestamp);
            }
            else if (rnd < 20 && timestamp.Hour < 21)
            {
                yield return ZoneTransition(storeNumber, "D", "C", timestamp);
            }
            else if (rnd < 40 && timestamp.Hour < 21)
            {
                yield return ZoneTransition(storeNumber, "D", "B", timestamp);
            }
            else
            {
                //yield return ZoneTransition(storeNumber, "A", "B", timestamp);
                yield return StoreExit(storeNumber, "D", timestamp);
            }
        }
    }


    private static TurnstilePassageDetected StoreEnter(int storeNumber, string toZone, DateTime timestamp)
    {
        return toZone switch
        {
            "D" => new TurnstilePassageDetected
            {
                Timestamp = timestamp,
                Turnstile = new TurnstileIdentifier { SerialNumber = $"{storeNumber}-{toZone}0" },
                Direction = TurnstileDirection.CounterClockwise
            },
            _ => new TurnstilePassageDetected
            {
                Timestamp = timestamp,
                Turnstile = new TurnstileIdentifier { SerialNumber = $"{storeNumber}-0{toZone}" },
                Direction = TurnstileDirection.Clockwise
            }
        };
    }

    private static TurnstilePassageDetected ZoneTransition(int storeNumber, string fromZone, string toZone, DateTime timestamp)
    {
        return new TurnstilePassageDetected
        {
            Timestamp = timestamp,
            Turnstile = new TurnstileIdentifier
            {
                SerialNumber = $"{storeNumber}-{fromZone}{toZone}"
            },
            Direction = TurnstileDirection.Clockwise
        };
    }

    private static TurnstilePassageDetected StoreExit(int storeNumber, string fromZone, DateTime timestamp)
    {
        return fromZone switch
        {
            "D" => new TurnstilePassageDetected
            {
                Timestamp = timestamp,
                Turnstile = new TurnstileIdentifier { SerialNumber = $"{storeNumber}-{fromZone}0" },
                Direction = TurnstileDirection.Clockwise
            },
            _ => new TurnstilePassageDetected
            {
                Timestamp = timestamp,
                Turnstile = new TurnstileIdentifier { SerialNumber = $"{storeNumber}-0{fromZone}" },
                Direction = TurnstileDirection.CounterClockwise
            }
        };
    }
}