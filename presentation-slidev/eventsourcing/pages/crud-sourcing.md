# Event Sourcing vs CRUD Sourcing

### Data centric instead of business events

Is the event something that makes sense to business?

<div class="grid grid-cols-2 gap-12">
<div>

#### CRUD Sourcing

```cs {none|1-4|6-8|none}
// Anemic remove event
record PlaylistDeleted {
  id: uuid, reason?: string
}

// Some soft-delete style
record PlaylistDeactivated {
  id: uuid, reason?: string
}
```

</div>
<div>

#### Event Sourcing

```cs {none|1-4|6-8}
// Names telling you why
record SubscriptionCancelled {
  id: uuid
}

record UserUnfollowedPlaylist {
  id: uuid
}
```

</div>
</div>



