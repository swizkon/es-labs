# Event Sourcing vs CRUD Sourcing

### Data centric instead of business events

Example managing a buffer of tasks to complete.\
Here in the shape of a list with tasks up for grabs.

<div class="grid grid-cols-2 gap-12">
<div>

#### CRUD Sourcing &#9763;

```cs {none|1-4|6-8|none}
// Anemic remove event
record TaskRemoved {
  id: Guid, reason?: string
}

// Some update
record StatusUpdated {
  id: Guid, status?: string
}
```

</div>
<div>

#### Event Sourcing &#9842;

```cs {none|1-4|6-8}
// Why item was removed from the read model
record TaskDeferred {
  id: Guid
}

record TaskPicked {
  id: Guid
}
```

</div>
</div>

Is the event something that makes sense to business?


