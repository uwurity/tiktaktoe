using System;
using Newtonsoft.Json;

namespace tiktaktoe.Messages;

[Serializable]
public class UpdateMessage
{
    [JsonProperty("position")] public BoardPosition? Position { get; set; }

    [JsonProperty("marked")] public Mark Marked { get; set; }

    [JsonProperty("mark")] public Mark Mark { get; set; }

    [JsonProperty("deadline")] public double Deadline { get; set; }

    [JsonProperty("moveCount")] public int MoveCount { get; set; }
}