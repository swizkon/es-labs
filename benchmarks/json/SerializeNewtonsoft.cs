using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace json;

public class SerializeNewtonsoft
{
    public T? DoIt<T>(T obj)
    {
        var s = JsonConvert.SerializeObject(obj);
        return Deserialize<T>(s);
    }

    private T? Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}