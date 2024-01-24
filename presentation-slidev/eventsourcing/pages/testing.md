---
title: Testing
---

# Testing
### Aggregate invariants and state transitions


<div class="grid grid-cols-2 gap-12">
<div>

## Use cases
 - Anomaly detection
 - Event-driven by nature
 - Temporal queries
 - DDD

## Characteristics
 - Capture the business domain lingo\
   (Domain Driven-style)
 - Probably not applied to the entire system
 - Append-only log of all domain events\
   (Many dbs works this way under the hood)




</div>
<div>

#### Benefits &hearts;
 - Capture facts now and decide later
 - Replay for new models
 - Run simulations
 - Caching for ever
 - Auditing-friendly by design
 - Hopefully alignment between business and dev team
 - Optimize for Write and Read\
   (with eventuall concistency as penalty)

#### Pains &#9760;
 - Versioning
 - Migrations
 - Extra code for events and projections
 - Naming things...
 - Stream strategy/partitioning (EventStoreDB)
 - Find concistency boundary


</div>
</div>


<hr />
Resources:

[A Beginner’s Guide to Event Sourcing](https://www.eventstore.com/event-sourcing)