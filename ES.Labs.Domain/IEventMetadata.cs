// using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

namespace ES.Labs.Domain;

public interface IEventMetadataInfo
{
    string GetCommitVersion();
}

public class EventDataBuilder
{
    private readonly IEventMetadataInfo _eventMetadataInfo;

    public EventDataBuilder(IEventMetadataInfo eventMetadataInfo)
    {
        _eventMetadataInfo = eventMetadataInfo;
    }

    public ReadOnlyMemory<byte> BuildMetadata(object data)
    {
        var metadata = new
        {
            Timestamp = DateTime.UtcNow.ToString("o"),
            CtrlType = data.GetType().FullName,
            data.GetType().AssemblyQualifiedName,
            CommitHash = _eventMetadataInfo.GetCommitVersion()
        };
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(metadata));
    }
}