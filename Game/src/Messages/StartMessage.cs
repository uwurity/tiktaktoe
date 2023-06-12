using System;
using Newtonsoft.Json;

namespace tiktaktoe.Messages;

[Serializable]
public class StartMessage
{
    [JsonProperty("marks")] public Marks Marks { get; set; } = null!;

    [JsonProperty("mark")] public Mark Mark { get; set; }

    [JsonProperty("deadline")] public double Deadline;
}