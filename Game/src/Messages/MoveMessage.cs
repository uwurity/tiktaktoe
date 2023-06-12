using System;
using Newtonsoft.Json;

namespace tiktaktoe.Messages;

[Serializable]
public class MoveMessage
{
    [JsonProperty("position")] public BoardPosition Position { get; set; } = null!;
}