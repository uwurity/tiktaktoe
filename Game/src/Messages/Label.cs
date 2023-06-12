using System;
using Newtonsoft.Json;

namespace tiktaktoe.Messages;

[Serializable]
public class Label
{
    [JsonProperty("creator")] public string Creator { get; set; } = null!;

    [JsonProperty("code")] public int Code { get; set; }

    [JsonProperty("level")] public Level Level { get; set; }

    [JsonProperty("open")] public int Open { get; set; }

    [JsonProperty("boardSize")] public BoardPosition BoardSize { get; set; } = null!;

    [JsonProperty("size")] public int Size { get; set; }

    public override string ToString()
        => $"+label.code:{Code} " +
           $"label.level:{(int)Level} " +
           $"label.open:{Open.ToString().ToLower()} " +
           $"label.boardSize.row:{BoardSize.Row} " +
           $"label.boardSize.col:{BoardSize.Col} ";
}