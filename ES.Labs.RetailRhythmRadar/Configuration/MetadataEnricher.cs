using EventSourcing;

namespace RetailRhythmRadar.Configuration;

public class MetadataEnricher : IEnrichMetaData
{
    public IDictionary<string, string> Enrich(IDictionary<string, string> metadata)
    {
        metadata.Add("GitVersion", VersionInfo.GitVersion);
        return metadata;
    }
}