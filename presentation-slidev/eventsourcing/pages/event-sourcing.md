---
title: Event Sourcing in a nutshell
---

# Event Sourcing in a nutshell
### Storing data as a series of events


<div class="grid grid-cols-2 gap-12">
<div>

## Use cases
 - Anomaly detection
 - Event-driven by nature
 - Audit log by design
 - Temporal queries
 - DDD

## Characteristics
 - Capture the business domain lingo\
   (Domain Driven-style)
 - Probably not applied to the entire system
 - More a "Design pattern" than "Architectural Pattern"
 - Append-only log of all domain events\
   (Many dbs works this way under the hood)




</div>
<div>

#### Benefits &hearts;
 - Capture facts now and decide later
 - Replay for new models
 - Run simulations
 - Cache for ever
 - Audit log by design
 - Hopefully alignment between business and dev team

#### Pains &#9760;
 - Versioning
 - Migrations
 - Extra code for events and projections
 - Naming things...

</div>
</div>
