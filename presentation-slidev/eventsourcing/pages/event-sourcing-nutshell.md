---
title: Event Sourcing in a nutshell
---

# Event Sourcing in a nutshell
### Storing data as a series of events




<div class="grid grid-cols-2 gap-12">
<div>

An append-only log of accepted business facts.\
State is derived from replay of all events inside an aggregate.

#### Use cases
Anomaly detection\
Temporal queries\


#### Characteristics
Capture the business domain lingo (Domain Driven-style)\
Probably not applied to the entire system



<hr />
Resources:

[A Beginner’s Guide to Event Sourcing](https://www.eventstore.com/event-sourcing)

</div>
<div>

#### Events
 - Facts that express business concepts
 - Past tense
 - Immutable

#### Streams
 - Consistency boundary (Aggregate)
 - Optimistic concurrency


#### Projections
 - Mapped from stream(s)

#### Subscriptions
 - Observe stream(s) and publish


</div>
</div>

