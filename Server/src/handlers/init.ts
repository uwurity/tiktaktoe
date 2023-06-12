import { State, tickRate } from "./common";

export const matchInit: nkruntime.MatchInitFunction<State> = function (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, params: {[key: string]: string}) {
    var state = params as unknown as State;

    logger.info("Creating a new match: %s for %s", ctx.matchId, state.label.creator);

    return {
        state,
        tickRate,
        label: JSON.stringify(state.label),
    };
}
