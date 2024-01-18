---
title: Event Sourcing - code patterns
---

# Event Sourcing
### Storing data as a series of events


<div class="grid grid-cols-2 gap-12">
<div>

## Characteristics
 - Capture the business domain lingo\
   (Domain Driven-style)
 - Probably not applied to the entire system
 - Audit log by design
 - More a "Design pattern" than "Architectural Pattern"
 - Append-only log of all domain events\
   (Many dbs works this way under the hood)
 - Cachable for ever
 - Temporal queries

</div>
<div>

#### Benefits
 - Capture facts now and decide later
 - Replay for new models
 - Run simulations
 - Hopefully alignment between business and dev team

#### Pains
 - Versioning
 - Migrations
 - Extra code for events and projections
 - Naming things...

</div>
</div>

<hr/>
LINKS HERE MAYBE...