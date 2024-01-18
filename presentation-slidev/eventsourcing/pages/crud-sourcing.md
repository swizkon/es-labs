# CRUD Sourcing

### Data centric instead of business events

Is the event something that makes sense to business?

```cs {none|1-4|6-9|11-18}
// Anemic remove event
record PlaylistDeleted {
  id: uuid, reason?: string
}

// Some soft-delete style
record PlaylistDeactivated {
  id: uuid, reason?: string
}

// Names telling you why
record SubscriptionCancelled {
  id: uuid
}

record UserUnfollowedPlaylist {
  id: uuid
}

```
