# Event Sourcing vs CRUD Sourcing

### Data centric instead of business events

Is the event something that makes sense to business?

<div class="grid grid-cols-2 gap-12">
<div>

#### CRUD Sourcing &#9763;

```cs {none|1-4|6-8|none}
// Anemic remove event
record PlaylistDeleted {
  id: Guid, reason?: string
}

// Some soft-delete style
record PlaylistDeactivated {
  id: Guid, reason?: string
}
```

</div>
<div>

#### Event Sourcing &#9842;

```cs {none|1-4|6-8}
// Names telling you why
record SubscriptionCancelled {
  id: Guid
}

record UserUnfollowedPlaylist {
  id: Guid
}
```

</div>
</div>



