using System;
using Newtonsoft.Json;

namespace tiktaktoe.Messages;

[Serializable]
public class MarkMetadata
{
    [JsonProperty("value")] public Mark Value { get; set; }

    [JsonProperty("order")] public int Order { get; set; }
}