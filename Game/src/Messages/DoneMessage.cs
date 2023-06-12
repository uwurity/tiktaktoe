using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace tiktaktoe.Messages;

[Serializable]
public class DoneMessage
{
    [JsonProperty("position")] public BoardPosition Position { get; set; } = null!;

    [JsonProperty("winner")] public Mark Winner { get; set; }

    [JsonProperty("winnerPositions")] public List<BoardPosition>? WinnerPositions { get; set; }

    [JsonProperty("nextGameStart")] public double NextGameStart { get; set; }
}