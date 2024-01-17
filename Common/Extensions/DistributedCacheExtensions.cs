using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Common.Extensions;

public static class DistributedCacheExtensions
{
    public static async Task<T> GetOrSetAsync<T>(this IDistributedCache cache, string key, Func<T> func)
    {
        var cached = await cache.GetStringAsync(key);
        if (cached != null)
        {
            return JsonSerializer.Deserialize<T>(cached) ?? func();
        }

        var result = func();
        await cache.SetStringAsync(key, JsonSerializer.Serialize(result));
        return result;
    }

    public static async Task<T> SetAsync<T>(this IDistributedCache cache, string key, Func<T> func)
    {
        var result = func();
        await cache.SetStringAsync(key, JsonSerializer.Serialize(result));
        return result;
    }
}