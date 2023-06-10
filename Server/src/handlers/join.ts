import { State, maxPlayers, msecToSec, tickRate } from ".";
import { DoneMessage, LeaveLobbyMessage, OpCode, RejoinMessage } from "../messages";

export let matchJoin: nkruntime.MatchJoinFunction<State> = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: State, presences: nkruntime.Presence[]) {
    const t = msecToSec(Date.now());

    for (const presence of presences) {
        state.emptyTicks = 0;
        state.presences[presence.userId] = presence;
        state.joinsInProgress--;
        logger.info("Player %s joined match %s", presence.userId, ctx.matchId);

        // Check if we must send a message to this user to update them on the current game state.
        if (state.playing) {
            // There's a game still currently in progress, the player is re-joining after a disconnect. Give them a state update.
            let rejoin: RejoinMessage = {
                board: state.board,
                marks: state.marks,
                mark: state.mark,
                deadline: t + Math.floor(state.deadlineRemainingTicks/tickRate),
            }
            // Send a message to the user that just joined.
            dispatcher.broadcastMessage(OpCode.REJOIN, JSON.stringify(rejoin));
        } else if (state.board.length !== 0 && Object.keys(state.marks).length !== 0 && state.marks[presence.userId]) {
            logger.debug("Player: %s rejoined match: %s", presence.userId, ctx.matchId);
            // There's no game in progress but we still have a completed game that the user was part of.
            // They likely disconnected before the game ended, and have since forfeited because they took too long to return.
            let done: DoneMessage = {
                position: { row: 0, col: 0},
                winner: state.winner,
                winnerPositions: state.winnerPositions,
                nextGameStart: t + Math.floor(state.nextGameRemainingTicks/tickRate)
            }
            // Send a message to the user that just joined.
            dispatcher.broadcastMessage(OpCode.DONE, JSON.stringify(done))
        }
    }

    // Check if match was open to new players, but should now be closed.
    if (Object.keys(state.presences).length === maxPlayers && state.label.open === 1) {
        state.label.open = 0;
        const labelJSON = JSON.stringify(state.label);
        dispatcher.matchLabelUpdate(labelJSON);

        // Notify players to leave the lobby
        logger.info("starting to leave lobby: %s", ctx.matchId);
        let leaveLobbyMsg: LeaveLobbyMessage = {}
        dispatcher.broadcastMessage(OpCode.LEAVE_LOBBY, JSON.stringify(leaveLobbyMsg));
    }

    return {state};
}
