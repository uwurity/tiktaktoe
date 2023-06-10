import { getNextMark, progressCheck } from ".";
import { State, calculateDeadlineTicks, delaybetweenGamesSec, tickRate } from "..";
import { DoneMessage, Mark, Message, MoveMessage, OpCode, Progress, UpdateMessage } from "../../messages";

export let handleMove = function(message: nkruntime.MatchMessage, ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, s: State, currentTime: number) {
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
    if (s.board[msg.position.row][msg.position.col]) {
        // Client sent a position outside the board, or one that has already been played.
        dispatcher.broadcastMessage(OpCode.REJECTED, null, [message.sender]);
        return;
    }

    // Update the game state.
    s.board[msg.position.row][msg.position.col] = mark.value;
    s.moveCount++;
    s.mark = getNextMark(s, mark.order)?.value ?? Mark.UNDEFINED;
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
            s.deadlineRemainingTicks = 0;
            s.nextGameRemainingTicks = delaybetweenGamesSec * tickRate;
            break;

        case Progress.IN_PROGRESS:
            opCode = OpCode.UPDATE
            outgoingMsg = {
                ...msg,
                marked: mark.value,
                mark: s.mark,
                deadline: currentTime + Math.floor(s.deadlineRemainingTicks/tickRate),
            } as UpdateMessage;
            break;
    }

    if (!s.playing)
    {
        opCode = OpCode.DONE
        outgoingMsg = {
            ...msg,
            winner: s.winner,
            winnerPositions: s.winnerPositions,
            nextGameStart: currentTime + Math.floor(s.nextGameRemainingTicks/tickRate),
        } as DoneMessage;
    }
    
    dispatcher.broadcastMessage(opCode, JSON.stringify(outgoingMsg));
}