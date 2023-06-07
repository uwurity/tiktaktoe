import { shuffle, last } from "lodash";
import { Board, BoardPosition, BoardSize, DoneMessage, Level, Mark, Message, MoveMessage, OpCode, StartMessage, UpdateMessage } from "./messages";

export const moduleName = "tiktaktoe";
export const tickRate = 5;
export const maxEmptySec = 30;
export const delaybetweenGamesSec = 5;
export const minCode = 100000;
export const maxCode = 999999;
export const marks = Object.values(Mark).filter(mark => mark !== Mark.UNDEFINED);
export const maxPlayer = marks.length;

export interface MatchLabel {
    code: number
    level: Level
    open: boolean
}

export interface State {
    // Match label
    label: MatchLabel
    // Ticks where no actions have occurred.
    emptyTicks: number
    // Currently connected users, or reserved spaces.
    presences: {[userId: string]: nkruntime.Presence | null}
    // Number of users currently in the process of connecting to the match.
    joinsInProgress: number
    // True if there's a game currently in progress.
    playing: boolean
    // Set board size
    size: BoardSize
    // Current state of the board.
    board: Board
    // Mark assignments to player user IDs.
    marks: {[userId: string]: { value: Mark | null, order: number }}
    // Whose turn it currently is.
    mark: Mark
    // Ticks until they must submit their move.
    deadlineRemainingTicks: number
    // The winner of the current game.
    winner: Mark | null
    // The winner positions.
    winnerPositions: BoardPosition[] | null
    // Ticks until the next game starts, if applicable.
    nextGameRemainingTicks: number
}

export let matchInit: nkruntime.MatchInitFunction<State> = function (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, params: {[key: string]: string}) {
    var level = Level[params['level'] as keyof typeof Level];

    if (level === undefined) level = Level.UNDEFINED;

    var label: MatchLabel = {
        code: getRndInclInteger(minCode, maxCode),
        level: level,
        open: true,
    };

    var state: State = {
        label: label,
        emptyTicks: 0,
        presences: {},
        joinsInProgress: 0,
        playing: false,
        size: { row: 0, col: 0 },
        board: [],
        marks: {},
        mark: Mark.UNDEFINED,
        deadlineRemainingTicks: 0,
        winner: null,
        winnerPositions: null,
        nextGameRemainingTicks: 0,
    };

    return {
        state,
        tickRate,
        label: "",
    };
}

export let matchJoinAttempt: nkruntime.MatchJoinAttemptFunction<State> = function (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: State, presence: nkruntime.Presence, metadata: {[key: string]: any}) {
    if (state.label.level === Level.UNDEFINED)
    {
        return {
            state: state,
            accept: false,
            rejectMessage: 'this level is not supported',
        }
    }

    // Check if it's a user attempting to rejoin after a disconnect.
    if (presence.userId in state.presences) {
        if (state.presences[presence.userId] === null) {
            // User rejoining after a disconnect.
            state.joinsInProgress++;
            return {
                state: state,
                accept: false,
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
    if (connectedPlayers(state) + state.joinsInProgress >= maxPlayer) {
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

export let matchJoin: nkruntime.MatchJoinFunction<State> = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: State, presences: nkruntime.Presence[]) {
    const t = msecToSec(Date.now());

    for (const presence of presences) {
        state.emptyTicks = 0;
        state.presences[presence.userId] = presence;
        state.joinsInProgress--;

        // Check if we must send a message to this user to update them on the current game state.
        if (state.playing) {
            // There's a game still currently in progress, the player is re-joining after a disconnect. Give them a state update.
            let update: UpdateMessage = {
                board: state.board,
                mark: state.mark,
                deadline: t + Math.floor(state.deadlineRemainingTicks/tickRate),
            }
            // Send a message to the user that just joined.
            dispatcher.broadcastMessage(OpCode.UPDATE, JSON.stringify(update));
        } else if (state.board.length !== 0 && Object.keys(state.marks).length !== 0 && state.marks[presence.userId]) {
            logger.debug('player %s rejoined game', presence.userId);
            // There's no game in progress but we still have a completed game that the user was part of.
            // They likely disconnected before the game ended, and have since forfeited because they took too long to return.
            let done: DoneMessage = {
                board: state.board,
                winner: state.winner,
                winnerPositions: state.winnerPositions,
                nextGameStart: t + Math.floor(state.nextGameRemainingTicks/tickRate)
            }
            // Send a message to the user that just joined.
            dispatcher.broadcastMessage(OpCode.DONE, JSON.stringify(done))
        }
    }

    // Check if match was open to new players, but should now be closed.
    if (Object.keys(state.presences).length >= 2 && state.label.open) {
        state.label.open = false;
        const labelJSON = JSON.stringify(state.label);
        dispatcher.matchLabelUpdate(labelJSON);
    }

    return {state};
}

export let matchLeave: nkruntime.MatchLeaveFunction<State> = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: State, presences: nkruntime.Presence[]) {
    for (let presence of presences) {
        logger.info("Player: %s left match: %s.", presence.userId, ctx.matchId);
        state.presences[presence.userId] = null;
    }

    return {state};
}

export let matchLoop: nkruntime.MatchLoopFunction<State> = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: State, messages: nkruntime.MatchMessage[]) {
    logger.debug('Running match loop. Tick: %d', tick);

    if (connectedPlayers(state) + state.joinsInProgress === 0) {
        state.emptyTicks++;
        if (state.emptyTicks >= maxEmptySec * tickRate) {
            // Match has been empty for too long, close it.
            logger.info('closing idle match');
            return null;
        }
    }

    let t = msecToSec(Date.now());

    // If there's no game in progress check if we can (and should) start one!
    if (!state.playing) {
        // Between games any disconnected users are purged, there's no in-progress game for them to return to anyway.
        for (let userID in state.presences) {
            if (state.presences[userID] === null) {
                delete state.presences[userID];
            }
        }

        // Check if we need to update the label so the match now advertises itself as open to join.
        if (Object.keys(state.presences).length < 2 && !state.label.open) {
            state.label.open = true;
            let labelJSON = JSON.stringify(state.label);
            dispatcher.matchLabelUpdate(labelJSON);
        }

        // Check if we have enough players to start a game.
        if (Object.keys(state.presences).length < 2) {
            return { state };
        }

        // Check if enough time has passed since the last game.
        if (state.nextGameRemainingTicks > 0) {
            state.nextGameRemainingTicks--;
            return { state };
        }

        // We can start a game! Set up the game state and assign the marks to each player.
        state.playing = true;
        state.board = new Array(9);
        state.marks = {};
        let randomMarks = shuffle(marks);
        state.mark = Mark[last(randomMarks) as keyof typeof Mark];
        Object.keys(state.presences).forEach(userId => {
            let order = randomMarks.length - 1;
            state.marks[userId] = {
                value: Mark[(randomMarks.shift() ?? "UNDEFINED") as keyof typeof Mark],
                order: order === -1 ? maxPlayer - 1 : order,
            };
        });
        state.winner = null;
        state.winnerPositions = null;
        state.deadlineRemainingTicks = calculateDeadlineTicks(state.label);
        state.nextGameRemainingTicks = 0;

        // Notify the players a new game has started.
        let msg: StartMessage = {
            board: state.board,
            marks: state.marks,
            mark: state.mark,
            deadline: t + Math.floor(state.deadlineRemainingTicks / tickRate),
        }
        dispatcher.broadcastMessage(OpCode.START, JSON.stringify(msg));

        return { state };
    }

    // There's a game in progress. Check for input, update match state, and send messages to clientstate.
    for (const message of messages) {
        switch (message.opCode) {
            case OpCode.MOVE:
                logger.debug('Received move message from user: %v', state.marks);
                let mark = state.marks[message.sender.userId] ?? null;
                if (mark === null || state.mark != mark.value) {
                    // It is not this player's turn.
                    dispatcher.broadcastMessage(OpCode.REJECTED, null, [message.sender]);
                    continue;
                }

                let msg = {} as MoveMessage;
                try {
                    msg = JSON.parse(nk.binaryToString(message.data));
                } catch (error) {
                    // Client sent bad data.
                    dispatcher.broadcastMessage(OpCode.REJECTED, null, [message.sender]);
                    logger.debug('Bad data received: %v', error);
                    continue;
                }
                if (state.board[msg.position.row][msg.position.col]) {
                    // Client sent a position outside the board, or one that has already been played.
                    dispatcher.broadcastMessage(OpCode.REJECTED, null, [message.sender]);
                    continue;
                }

                // Update the game state.
                state.board[msg.position.row][msg.position.col] = mark.value;
                state.mark = getNextMark(state, mark.order)?.value ?? Mark.UNDEFINED;
                state.deadlineRemainingTicks = calculateDeadlineTicks(state.label);

                // Check if game is over through a winning move.
                const [winner, winningPos] = winCheck(state.board, mark.value);
                if (winner) {
                    state.winner = mark.value;
                    state.winnerPositions = winningPos;
                    state.playing = false;
                    state.deadlineRemainingTicks = 0;
                    state.nextGameRemainingTicks = delaybetweenGamesSec * tickRate;
                }
                // Check if game is over because no more moves are possible.
                let tie = state.board.every(row => row !== null && row.every(col => col !== null));
                if (tie) {
                    // Update state to reflect the tie, and schedule the next game.
                    state.playing = false;
                    state.deadlineRemainingTicks = 0;
                    state.nextGameRemainingTicks = delaybetweenGamesSec * tickRate;
                }

                let opCode: OpCode
                let outgoingMsg: Message
                if (state.playing) {
                    opCode = OpCode.UPDATE
                    let msg: UpdateMessage = {
                        board: state.board,
                        mark: state.mark,
                        deadline: t + Math.floor(state.deadlineRemainingTicks/tickRate),
                    }
                    outgoingMsg = msg;
                } else {
                    opCode = OpCode.DONE
                    let msg: DoneMessage = {
                        board: state.board,
                        winner: state.winner,
                        winnerPositions: state.winnerPositions,
                        nextGameStart: t + Math.floor(state.nextGameRemainingTicks/tickRate),
                    }
                    outgoingMsg = msg;
                }
                dispatcher.broadcastMessage(opCode, JSON.stringify(outgoingMsg));
                break;
            default:
                // No other opcodes are expected from the client, so automatically treat it as an error.
                dispatcher.broadcastMessage(OpCode.REJECTED, null, [message.sender]);
                logger.error('Unexpected opcode received: %d', message.opCode);
                break;
        }
    }

    // Keep track of the time remaining for the player to submit their move. Idle players forfeit.
    if (state.playing) {
        state.deadlineRemainingTicks--;
        if (state.deadlineRemainingTicks <= 0 ) {
            // The player has run out of time to submit their move.
            state.playing = false;
            state.winner = state.mark === Mark.O ? Mark.X : Mark.O;
            state.deadlineRemainingTicks = 0;
            state.nextGameRemainingTicks = delaybetweenGamesSec * tickRate;

            let msg: DoneMessage = {
                board: state.board,
                winner: state.winner,
                nextGameStart: t + Math.floor(state.nextGameRemainingTicks/tickRate),
                winnerPositions: null,
            }
            dispatcher.broadcastMessage(OpCode.DONE, JSON.stringify(msg));
        }
    }

    return { state };
}

export let matchTerminate: nkruntime.MatchTerminateFunction<State> = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: State, graceSeconds: number) {
    logger.debug('Match terminated: %s', ctx.matchId);
    return { state };
}

export let matchSignal: nkruntime.MatchSignalFunction<State> = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: State) {
    return { state };
}

function getNextMark(s: State, order: number)
{
    return Object.values(s.marks).find(mark => mark.order == order + 1);
}

function getRndInclInteger(min: number, max: number) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

function connectedPlayers(s: State): number {
    let count = 0;
    for(const p of Object.keys(s.presences)) {
        if (s.presences[p] !== null) {
            count++;
        }
    }
    return count;
}

function msecToSec(n: number): number {
    return Math.floor(n / 1000);
}
function calculateDeadlineTicks(label: MatchLabel): number {
    throw new Error("Function not implemented.");
}

function winCheck(board: Board, mark: Mark): [any, any] {
    throw new Error("Function not implemented.");
}

