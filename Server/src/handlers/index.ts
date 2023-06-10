import { fill, times } from "lodash";
import { Board, BoardPosition, BoardSize, Level, Mark } from "../messages";
import { matchInit } from "./init";
import { matchJoin } from "./join";
import { matchJoinAttempt } from "./joinAttempt";
import { matchLeave } from "./leave";
import { matchLoop } from "./loop";
import { matchSignal } from "./signal";
import { matchTerminate } from "./terminate";

export const moduleName = "tiktaktoe";
export const tickRate = 5;
export const maxEmptySec = 30;
export const maxStaleSec = 120;
export const delaybetweenGamesSec = 5;
export const turnTimeAdventureSec = 10;
export const turnTimeClassicSec = 20;
export const maxAdventureBoardSize: BoardSize = { row: 15, col: 15 };
export const minAdventureBoardSize: BoardSize = { row: 5, col: 5 };
export const maxClassicBoardSize: BoardSize = { row: 5, col: 5 };
export const minClassicBoardSize: BoardSize = { row: 3, col: 3 };
export const minCode = 100000;
export const maxCode = 999999;
export const maxWinLength = 5;
export const marks = Object.keys(Mark).filter(key => isNaN(Number(key)) && key !== "UNDEFINED");
export const maxPlayers = marks.length;
export const minPlayers = marks.length;

export interface MatchLabel {
    /**
     * the one who created the match
     * @format uuid
     */
    creator: string,

    /**
     * @minimum 100000
     * @maximum 999999
     */
    code: number

    level: Level

    /**
     * @minimum 0
     * @maximum 1
     */
    open: number

    // Length of winning sequence.
    winLength: number

    // Specify board size
    boardSize: BoardSize
}

export interface State {
    // Match label
    label: MatchLabel
    // Ticks where no actions have occurred.
    emptyTicks: number
    // Ticks where the last action have occured
    staleTicks: number
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
    // Number of moves taken.
    moveCount: number
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

export function getRndInclInteger(min: number, max: number) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

export function calculateDeadlineTicks(l: MatchLabel): number {
    switch (l.level)
    {
        case Level.CLASSIC:
            return turnTimeClassicSec * tickRate;
        case Level.ADVENTURE:
            return turnTimeAdventureSec * tickRate;
        default:
            return 10 * tickRate;
    }
}

export function create2DArray(row: number, col: number): any[][] {
    return times(row, () => fill(Array(col), null));
}

export function connectedPlayers(s: State): number {
    return Object.keys(s.presences).length;
}

export function msecToSec(n: number): number {
    return Math.floor(n / 1000);
}

const handlers = { matchInit, matchJoin, matchJoinAttempt, matchLeave, matchLoop, matchSignal, matchTerminate };

export default handlers;
