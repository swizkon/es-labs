# Projections

### Modelling state from event streams

In-memory, cached or drop create schema in DB.

Motivator:\
Facts (events) dont change, but your perception of the facts do


```cs {none|1-6|8-16}
// Full projection
record CurrentUserInfo {
  id: number
  firstName: string
  lastName: string
}

record Leaderboard {
  game: string
  entries: Player[]
}

record Player {
  name: string
  score: number
}

```
