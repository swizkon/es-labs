# BONUS: Participant pattern

### Like an Actor Model or a MicroService (tm)...

A participant in a system is an that can do one or many of:

* Act on a command 
* Respond to a request
* React to an Event

```csharp
class MyParticipant :
    IHandle<SomeCommand>,
    IRespondTo<GetSomeStuff, TheResponseType>,
    IReactTo<SomeEvent>
{

}
```

## Drive
Keep Single Responsibility at a sane level.

