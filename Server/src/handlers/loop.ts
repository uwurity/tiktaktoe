import { State, calculateDeadlineTicks, connectedPlayers, create2DArray, delaybetweenGamesSec, getMaxPlayers, marks, maxEmptySec, msecToSec, tickRate } from "./common";
import { Board, BoardPosition, DoneMessage, Mark, Message, MoveMessage, OpCode, Progress, StartMessage, UpdateMessage } from "../messages";
import { inRange, last, merge, shuffle, take } from "lodash";

export const matchLoop: nkruntime.MatchLoopFunction<State> = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: State, messages: nkruntime.MatchMessage[]) {
    logger.debug("Running match loop for match: %s. Tick: %d", ctx.matchId, tick);

    if (connectedPlayers(state) + state.joinsInProgress === 0) {
        state.emptyTicks++;
        if (state.emptyTicks >= maxEmptySec * tickRate) {
            // Match has been empty for too long, close it.
            logger.info("Closing idle match: %s", ctx.matchId);
            return null;
        }
    }

    if (connectedPlayers(state) + state.joinsInProgress < getMaxPlayers(state.label.level)) {
        return { state };
    }

    let t = msecToSec(Date.now());

    // There's a game in progress. Check for input, update match state, and send messages to clientstate.
    for (const message of messages) {
        switch (message.opCode) {
            case OpCode.READY:
                state.ready?.push(message.sender.userId);
                // If there's no game in progress and everyone is ready, check if we can (and should) start one!
                if (!state.playing && state.ready?.length === getMaxPlayers(state.label.level)) {
                    return { state: startGame(state, t, dispatcher, logger), };
                }
                break;

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
            state.mark = getNextMark(state, state.mark)?.value ?? Mark.X;
            state.deadlineRemainingTicks = calculateDeadlineTicks(state.label);

            let msg: UpdateMessage = {
                marked,
                position: null,
                mark: state.mark,
                deadline: t + Math.floor(state.deadlineRemainingTicks/tickRate),
                moveCount: state.moveCount,
            };

            dispatcher.broadcastMessage(OpCode.UPDATE, JSON.stringify(msg));
        }
    }

    return { state };
}

let handleMove = function(message: nkruntime.MatchMessage, ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, s: State, currentTime: number) {
    logger.debug("Received move message from user: %s in match: %s", message.sender.userId, ctx.matchId);
    let mark = s.marks[message.sender.userId] ?? null;
    if (mark === null || s.mark != mark.value) {
        // It is not this player's turn.
        logger.debug("Rejected move message from user: %s", message.sender.userId);
        dispatcher.broadcastMessage(OpCode.REJECTED, null, [message.sender]);
        return;
    }

    let msg = {} as MoveMessage;
    try {
        msg = JSON.parse(nk.binaryToString(message.data));
    } catch (error) {
        // Client sent bad data.
        dispatcher.broadcastMessage(OpCode.REJECTED, null, [message.sender]);
        logger.debug("Bad data received: %s from user: %s", error);
        return;
    }
    if (!inRange(msg.position.row, 0, s.label.boardSize.row) || !inRange(msg.position.col, 0, s.label.boardSize.col)
    || s.board[msg.position.row][msg.position.col] !== null) {
        // Client sent a position outside the board, or one that has already been played.
        dispatcher.broadcastMessage(OpCode.REJECTED, null, [message.sender]);
        return;
    }

    // Update the game state.
    s.board[msg.position.row][msg.position.col] = mark.value;
    s.moveCount++;
    s.mark = getNextMark(s, mark.order)?.value ?? Mark.X;
    s.deadlineRemainingTicks = calculateDeadlineTicks(s.label);

    // Check if game is over through a winning move.
    let opCode: OpCode = OpCode.UPDATE;
    let outgoingMsg: Message = {};
    const [progress, winner, winningPos] = progressCheck(s.board, s.moveCount, msg.position, mark.value, s.label.winLength);

    switch (progress)
    {
        case Progress.WIN:
            s.winner = winner;
            s.winnerPositions = winningPos;
            s.playing = false;
            s.deadlineRemainingTicks = 0;
            s.nextGameRemainingTicks = delaybetweenGamesSec * tickRate;
            break;

        case Progress.TIE:
            // Update state to reflect the tie, and schedule the next game.
            s.playing = false;
            s.winner = mark.value;
            s.deadlineRemainingTicks = 0;
            s.nextGameRemainingTicks = delaybetweenGamesSec * tickRate;
            break;

        case Progress.IN_PROGRESS:
            opCode = OpCode.UPDATE
            outgoingMsg = merge(msg, {
                marked: mark.value,
                mark: s.mark,
                deadline: currentTime + Math.floor(s.deadlineRemainingTicks/tickRate),
                moveCount: s.moveCount
            }) as UpdateMessage;
            break;
    }

    if (!s.playing)
    {
        opCode = OpCode.DONE
        outgoingMsg = merge(msg, {
            winner: s.winner,
            winnerPositions: s.winnerPositions,
            nextGameStart: currentTime + Math.floor(s.nextGameRemainingTicks/tickRate),
        }) as DoneMessage;
    }
    
    const outgoingMsgString = JSON.stringify(outgoingMsg);
    logger.info("Sending message to match %s: %s", ctx.matchId, outgoingMsgString)
    dispatcher.broadcastMessage(opCode, outgoingMsgString);
}

let startGame = function(s: State, currentTime: number, dispatcher: nkruntime.MatchDispatcher, logger: nkruntime.Logger): State {
    // Between games any disconnected users are purged, there's no in-progress game for them to return to anyway.
    for (let userID in s.presences) {
        if (s.presences[userID] === null) {
            delete s.presences[userID];
        }
    }

    // Check if we have enough players to start a game.
    if (Object.keys(s.presences).length < getMaxPlayers(s.label.level)) {
        return s;
    }

    // Check if enough time has passed since the last game.
    if (s.nextGameRemainingTicks > 0) {
        s.nextGameRemainingTicks--;
        return s;
    }

    // We can start a game! Set up the game state and assign the marks to each player.
    s.playing = true;
    s.board = create2DArray(s.label.boardSize.row, s.label.boardSize.col);
    s.marks = {};
    let randomMarks = take(shuffle(marks), s.label.size);
    logger.info("Random marks length %d", randomMarks.length);
    s.mark = Mark[last(randomMarks) as keyof typeof Mark];
    Object.keys(s.presences).forEach(userId => {
        let order = randomMarks.length - 1;
        s.marks[userId] = {
            value: Mark[(randomMarks.shift() ?? "UNDEFINED") as keyof typeof Mark],
            order,
        };
    });
    Object.keys(s.marks).forEach(userId => {
        logger.info("User %s has Mark %s", userId, s.marks[userId].value?.toString);
    });
    s.winner = null;
    s.winnerPositions = null;
    s.deadlineRemainingTicks = calculateDeadlineTicks(s.label);
    s.nextGameRemainingTicks = 0;

    // Notify the players a new game has started.
    let msg: StartMessage = {
        marks: s.marks,
        mark: s.mark,
        deadline: currentTime + Math.floor(s.deadlineRemainingTicks / tickRate),
    }
    dispatcher.broadcastMessage(OpCode.START, JSON.stringify(msg));

    return s;
}

function getNextMark(s: State, order: number): { order: number, value: Mark | null } | undefined {
    return Object.values(s.marks).find(mark => mark.order == order + 1) ?? Object.values(s.marks).find(mark => mark.order == 0);
}

function progressCheck(board: Board, moveCount: number, lastMove: BoardPosition, mark: Mark, winLength: number): [Progress, Mark | null, [BoardPosition, BoardPosition] | null] {
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
