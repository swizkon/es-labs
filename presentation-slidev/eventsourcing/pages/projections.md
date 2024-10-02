# Modeling state from event streams

### Projections, View models, Read models

In-memory, dist cache, vendor specific or drop/create/migrate schema in DB.

Why? Facts (events) dont change, but your view of the facts might


<hr/>

#### Optimized read model

Select just enough data that is needed for views.

 - Use 1N, 2N, 3N form pending on use case
 - Star schema
 - Choose suitable format

```
fromStream("$ce-stores")
.when({
  $init: function() {
    return {
      stores: {
        "1":0,
        "2":0,
        "3":0,
        "4":0,
        "5":0,
      },
      enter: 0,
      exit: 0
    }
  },
  storeEnteredEvent: function(state, event) {
    state.stores[event.data.Store] += 1;
    state.enter += 1;
  },
  storeExitedEvent: function(state, event) {
    state.stores[event.data.Store] -= 1;
    state.exit += 1;
  }
})
.transformBy(function(state) {
  state.eof = 'yes';
})
```

<hr/>

[Projections Explained](https://www.youtube.com/watch?v=b2kSlDcAcps)