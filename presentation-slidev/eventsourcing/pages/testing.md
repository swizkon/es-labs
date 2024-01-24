---
title: Testing
---

# Pure functional projection testing
### Checking invariants and state transitions


<div class="grid grid-cols-2 gap-12">
<div>

#### Test

```cs
[Fact]
public void It_should_calculate_avg_time_per_customer()
{
  // Given
  var randomTime = GetRandomTime();
  FivePeopleEnteredAtSameTime(randomTime)

  // When
  .PeopleExitedAt(
    randomTime.AddSeconds(10),
    randomTime.AddSeconds(20),
    randomTime.AddSeconds(36))
    
  // Then
  .AverageTime.Should().Be(TimeSpan.FromSeconds(22));
}
```


</div>
<div>

#### Implementation

```cs
public static AvgTimeProjection Empty => new("1");

public AvgTimeProjection ApplyEvent(StoreEntered e)
{
  return this with
  {
    TotalOfVisitors = TotalOfVisitors + 1,
    CurrentNumberOfVisitors = CurrentNumberOfVisitors + 1,
    Entries = Entries.Append(e.Timestamp).ToList()
  };
}
```

</div>
</div>
