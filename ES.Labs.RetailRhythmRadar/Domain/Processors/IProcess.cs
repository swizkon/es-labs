using EventSourcing;

namespace RetailRhythmRadar.Domain.Processors;

public interface IProcess<in T>
{
    Task<IEnumerable<WriteEventResult>> Handle(T context);
}