using System;
using Newtonsoft.Json;

namespace tiktaktoe.Messages;

[Serializable]
public class BoardPosition
{
    [JsonProperty("row")] public int Row { get; set; }

    [JsonProperty("col")] public int Col { get; set; }

    public override string ToString()
        => $"{nameof(BoardPosition)}({nameof(Row)} = {Row}, {nameof(Col)} = {Col})";
}