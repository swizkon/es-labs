namespace EventSourcing;

public interface IEnrichMetaData
{
    Task<IDictionary<string,string>> EnrichAsync(IDictionary<string, string> metadata);
}