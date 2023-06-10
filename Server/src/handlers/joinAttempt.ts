import { State, connectedPlayers, maxPlayers } from ".";

export let matchJoinAttempt: nkruntime.MatchJoinAttemptFunction<State> = function (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: State, presence: nkruntime.Presence, metadata: {[key: string]: any}) {
    if (!state.label.open)
    {
        if (presence.userId !== state.label.creator)
        {
            logger.info("User %s attempted to join a closed room: %s", ctx.userId, ctx.matchId)
            return {
                state,
                accept: false,
                rejectMessage: "this room is not open"
            };
        }
        // the host has joined, open the room for everyone else
        state.label.open = 1;
        dispatcher.matchLabelUpdate(JSON.stringify(state.label));
    }

    // Check if it's a user attempting to rejoin after a disconnect.
    if (presence.userId in state.presences) {
        if (state.presences[presence.userId] === null && state.marks[presence.userId].value !== null) {
            // User rejoining after a disconnect.
            logger.info("Player %s is rejoining match %s", presence.userId, ctx.matchId);
            state.joinsInProgress++;
            return {
                state: state,
                accept: true,
            }
        } else {
            // User attempting to join from 2 different devices at the same time.
            return {
                state: state,
                accept: false,
                rejectMessage: 'already joined',
            }
        }
    }

    // Check if match is full.
    if (connectedPlayers(state) + state.joinsInProgress >= maxPlayers) {
        return {
            state: state,
            accept: false,
            rejectMessage: 'match full',
        };
    }

    // New player attempting to connect.
    state.joinsInProgress++;
    return {
        state,
        accept: true,
    }
}
