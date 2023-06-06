import { moduleName } from "./match_handler";

export function rpcCreateMatch(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
    var matchId = nk.matchCreate(moduleName, {});
    return JSON.stringify({ matchId });
}
