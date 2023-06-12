using System;
using Newtonsoft.Json;

namespace tiktaktoe.Messages;

[Serializable]
public class RejoinMessage
{
    [JsonProperty("board")] public Board Board { get; set; } = null!;

    [JsonProperty("marks")] public Marks Marks { get; set; } = null!;

    [JsonProperty("mark")] public Mark Mark { get; set; }

    [JsonProperty("deadline")] public double Deadline { get; set; }
}