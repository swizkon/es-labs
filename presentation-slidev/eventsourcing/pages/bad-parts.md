# The bad parts

### Weak events NOT capturing important business events

What is the reason for the state to change?

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
