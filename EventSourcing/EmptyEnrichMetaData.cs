namespace EventSourcing;

public class EmptyEnrichMetaData : IEnrichMetaData
{
    public IDictionary<string, string> Enrich(IDictionary<string, string> metadata) => metadata;
}