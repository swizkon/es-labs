
namespace json;

public class SerializeSystemText
{
    public T? DoIt<T>(T obj)
    {
        var s = System.Text.Json.JsonSerializer.Serialize(obj);
        return Deserialize<T>(s);
    }

    private T? Deserialize<T>(string json)
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(json);
    }
}