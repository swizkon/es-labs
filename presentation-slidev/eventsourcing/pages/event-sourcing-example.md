# Event Sourcing

### Story telling with actions and events

In this example we are working within the playlists context from some Streaming platform

```cs {none|1-4|6-9|11-17}
// Song added for some reason
record SongAdded {
  song_id: uuid, reason?: string
}

// Some CTA initiated from the great suggestions from AI
record SongSuggestionAccepted {
  song_id: uuid
}

record SongAddedFromRadioMode {
  song_id: uuid, radio_context: string
}
```