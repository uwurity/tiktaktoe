using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nakama;
using tiktaktoe.Messages;

namespace tiktaktoe.Utils;

public static class RpcHelper
{
    public static async Task<string> CreateMatch(this IClient client, ISession session, Label label)
    {
        var labelJson = label.ToJson();
        var result = await client.RpcAsync(session, "create_match", labelJson);
        var parsed = result.Payload.FromJson<Dictionary<string, string>>();
        return parsed == null ? Guid.Empty.ToString() : parsed["matchId"];
    }
}