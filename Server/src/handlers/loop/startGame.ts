import { last, shuffle } from "lodash";
import { State, calculateDeadlineTicks, create2DArray, marks, maxPlayers, tickRate } from "..";
import { Mark, OpCode, StartMessage } from "../../messages";

export let startGame = function(s: State, currentTime: number, dispatcher: nkruntime.MatchDispatcher): State {
    // Between games any disconnected users are purged, there's no in-progress game for them to return to anyway.
    for (let userID in s.presences) {
        if (s.presences[userID] === null) {
            delete s.presences[userID];
        }
    }

    // Check if we have enough players to start a game.
    if (Object.keys(s.presences).length < maxPlayers) {
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
    let randomMarks = shuffle(marks);
    s.mark = Mark[last(randomMarks) as keyof typeof Mark];
    Object.keys(s.presences).forEach(userId => {
        let order = randomMarks.length - 1;
        s.marks[userId] = {
            value: Mark[(randomMarks.shift() ?? "UNDEFINED") as keyof typeof Mark],
            order: order === -1 ? maxPlayers - 1 : order,
        };
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
