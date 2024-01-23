namespace EventSourcing;

public interface IProcess<in T>
{
    Task<IEnumerable<WriteEventResult>> Handle(T context);
}