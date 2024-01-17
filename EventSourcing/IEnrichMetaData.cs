namespace EventSourcing;

public interface IEnrichMetaData
{
    IDictionary<string,string> Enrich(IDictionary<string, string> metadata);
}