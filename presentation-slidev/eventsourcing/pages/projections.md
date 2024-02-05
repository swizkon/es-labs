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


<hr/>

[Projections Explained](https://www.youtube.com/watch?v=b2kSlDcAcps)