import { State, calculateDeadlineTicks, connectedPlayers, maxEmptySec, msecToSec, tickRate } from "..";
import { Board, BoardPosition, Mark, OpCode, Progress, UpdateMessage } from "../../messages";
import { handleMove } from "./handleMove";
import { startGame } from "./startGame";

export let matchLoop: nkruntime.MatchLoopFunction<State> = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: State, messages: nkruntime.MatchMessage[]) {
    logger.debug("Running match loop for match: %s. Tick: %d", ctx.matchId, tick);

    if (connectedPlayers(state) + state.joinsInProgress === 0) {
        state.emptyTicks++;
        if (state.emptyTicks >= maxEmptySec * tickRate) {
            // Match has been empty for too long, close it.
            logger.info("Closing idle match: %s", ctx.matchId);
            return null;
        }
    }

    let t = msecToSec(Date.now());

    // If there's no game in progress check if we can (and should) start one!
    if (!state.playing) {
        return { state: startGame(state, t, dispatcher) };
    }

    // There's a game in progress. Check for input, update match state, and send messages to clientstate.
    for (const message of messages) {
        switch (message.opCode) {
            case OpCode.MOVE:
                handleMove(message, ctx, logger, nk, dispatcher, state, t);
                break;

            default:
                // No other opcodes are expected from the client, so automatically treat it as an error.
                dispatcher.broadcastMessage(OpCode.REJECTED, null, [message.sender]);
                logger.error("Unexpected opcode received: %d", message.opCode);
                break;
        }
    }

    // Keep track of the time remaining for the player to submit their move.
    if (state.playing) {
        state.deadlineRemainingTicks--;
        // Idle players skip their turns.
        if (state.deadlineRemainingTicks <= 0) {
            // The player has run out of time to submit their move.
            let marked = state.mark;

            // Move on to the next mark
            state.mark = getNextMark(state, state.mark)?.value ?? Mark.UNDEFINED;
            state.deadlineRemainingTicks = calculateDeadlineTicks(state.label);

            let msg: UpdateMessage = {
                marked,
                position: null,
                mark: state.mark,
                deadline: t + Math.floor(state.deadlineRemainingTicks/tickRate),
            };

            dispatcher.broadcastMessage(OpCode.DONE, JSON.stringify(msg));
        }
    }

    return { state };
}

export function getNextMark(s: State, order: number) {
    return Object.values(s.marks).find(mark => mark.order == order + 1);
}

export function progressCheck(board: Board, moveCount: number, lastMove: BoardPosition, mark: Mark, winLength: number): [Progress, Mark | null, [BoardPosition, BoardPosition] | null] {
    const n = board.length;
    const m = board[0].length;
    const dirs: [number, number][] = [[0, 1], [1, 0], [1, 1], [1, -1]];  // horizontal, vertical, diagonal, antidiagonal
    
    for (const [dx, dy] of dirs) {
        let count = 1;
        let start: BoardPosition = { row: lastMove.row, col: lastMove.col }; // start with last move
        let end: BoardPosition = { row: lastMove.row, col: lastMove.col }; // end also starts with last move

        // check in both directions: forward and backward
        for (let dir = -1; dir <= 1; dir += 2) {
            let x = lastMove.row + dir * dx;
            let y = lastMove.col + dir * dy;
        
            while (x >= 0 && x < n && y >= 0 && y < m && board[x][y] === mark) {
                count++;
                // update the start or end point depending on direction
                if (dir == 1) {
                    end = { row: x, col: y };
                } else {
                    start = { row: x, col: y };
                }
                x += dir * dx;
                y += dir * dy;
            }
        }
        
        if (count >= winLength) {
            return [Progress.WIN, mark, [start, end]]; // return the start and end coordinates of the winning sequence
        }
    }

    if (moveCount == n * m) {
        return [Progress.TIE, null, null];
    }
  
    return [Progress.IN_PROGRESS, null, null];
}
