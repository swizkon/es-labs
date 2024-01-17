using System.Runtime.InteropServices;
using EventSourcing;

namespace RetailRhythmRadar.Configuration;

public class MetadataEnricher : IEnrichMetaData
{
    public IDictionary<string, string> Enrich(IDictionary<string, string> metadata)
    {
        metadata.Add("GitVersion", VersionInfo.GitVersion);
        metadata.Add("MachineName", Environment.MachineName);
        metadata.Add("OSVersion", Environment.OSVersion.VersionString);
        metadata.Add("Version", Environment.Version.ToString());
        
        metadata.Add("FrameworkDescription", RuntimeInformation.FrameworkDescription);
        metadata.Add("OSDescription", RuntimeInformation.OSDescription);
        metadata.Add("OSArchitecture", RuntimeInformation.OSArchitecture.ToString());
        metadata.Add("ProcessArchitecture", RuntimeInformation.ProcessArchitecture.ToString());
        
        return metadata;
    }
}