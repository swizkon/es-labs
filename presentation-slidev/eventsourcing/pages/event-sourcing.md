# Event Sourcing

### Storing data as a series of events 

Append-only log of all domain events




```cs {none|1-6|9|all}
// Anemic update event
record UserUpdated {
  id: number
  firstName: string
  lastName: string
}

function updateUser(id: number, update: User) {
  const user = getUser(id)
  const newUser = { ...user, ...update }
  saveUser(id, newUser)
}
```
