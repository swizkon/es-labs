# Event Sourcing vs Event streaming

### Persistent with ordering guarantee or durable just enough to enable catch-up?

Event streaming / pubsub with durable storage.\
IRL Kafka without topic expiration.

A little bit of a round peg/square hole that works, until it doesnt.\
Issues with messaging ordering might be very hard to handle.
