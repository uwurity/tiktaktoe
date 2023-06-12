using Newtonsoft.Json;

namespace tiktaktoe.Utils;

public static class JsonHelper
{
    public static string ToJson<T>(this T obj) => JsonConvert.SerializeObject(obj);

    public static T? FromJson<T>(this string json) => JsonConvert.DeserializeObject<T>(json);
}