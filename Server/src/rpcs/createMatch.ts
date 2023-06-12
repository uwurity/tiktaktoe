import { inRange } from "lodash";
import { MatchLabel, State, getRndInclInteger, maxAdventurePlayers, maxClassicBoardSize, maxClassicPlayers, maxCode, maxWinLength, minAdventureBoardSize, minAdventurePlayers, minClassicPlayers, minCode, moduleName } from "../handlers/common";
import { Level, Mark } from "../messages";

export const id = "create_match";

export function rpcCreateMatch(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
    var params: {[key: string]: string} = JSON.parse(payload);

    var label = params as unknown as MatchLabel;

    if (label.creator !== ctx.userId)
    {
        // a match should always be owned by the one requested rpcCreateMatch.
        logger.warn("User %s is attempting to create a match on behalf of user %s. Ignoring label.creator", ctx.userId, label.creator);
    }

    if (label.level === undefined)
    {
        // match level should be agreed upon creating match.
        logger.warn("User %s attempted to create a match with unknown level", ctx.userId);
        return JSON.stringify({ matchId: null });
    }

    const defaultBoardSize = label.level === Level.CLASSIC ? maxClassicBoardSize : minAdventureBoardSize;

    let size = 0;

    switch (label.level)
    {
        case Level.CLASSIC:
            size = inRange(label.size, minClassicPlayers, maxClassicPlayers) ? label.size : maxClassicPlayers;
            break;
        case Level.ADVENTURE:
            size = inRange(label.size, minAdventurePlayers, maxAdventurePlayers) ? label.size : maxAdventurePlayers;
            break;
    }

    var finalLabel: MatchLabel = {
        creator: ctx.userId,
        code: Number.isInteger(label.code) && inRange(label.code, minCode, maxCode) ? label.code : getRndInclInteger(minCode, maxCode),
        level: label.level,
        // the match will be closed by default, the host must first join the match in order to open the match to everyone.
        open: 0,
        winLength: label.winLength ?? maxWinLength,
        boardSize: label.boardSize ?? defaultBoardSize,
        size,
    };

    var state: State = {
        label: finalLabel,
        emptyTicks: 0,
        staleTicks: 0,
        presences: {},
        joinsInProgress: 0,
        playing: false,
        board: [],
        moveCount: 0,
        marks: {},
        mark: Mark.X,
        deadlineRemainingTicks: 0,
        winner: null,
        winnerPositions: null,
        nextGameRemainingTicks: 0,
        ready: [],
    };

    var matchId = nk.matchCreate(moduleName, state);
    logger.info("Created a match: %s for %s", matchId, ctx.userId);

    return JSON.stringify({ matchId });
}

export default {id, rpc: rpcCreateMatch};
