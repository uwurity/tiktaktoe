export enum Mark {
    X = 0,
    O = 1,
    ADD = 2,
    EQ = 3
}

export enum Level {
    CLASSIC = 1,
    ADVENTURE = 2
}

// The complete set of opcodes used for communication between clients and server.
export enum OpCode {
    // Leave the lobby.
    LEAVE_LOBBY = 0,
    // Join the lobby
    JOIN_LOBBY = 1,
	// New game round starting.
	START = 2,
    // User is rejoining the game.
    REJOIN = 3,
	// Update to the state of an ongoing round.
	UPDATE = 4,
    // Update to the game label.
    LABEL = 5,
	// A game round has just completed.
	DONE = 6,
	// A move the player wishes to make and sends to the server.
	MOVE = 7,
	// Move was rejected.
	REJECTED = 8,
    // Player is ready.
    READY = 9,
}

export enum Progress {
    WIN = 1,
    TIE = 2,
    IN_PROGRESS = 3,
}

export interface BoardPosition {
    /**
     * @exclusiveMinimum 0
     * @exclusiveMaximum 10
     */
    row: number

    /**
     * @minimum 0
     * @maximum 10
     */
    col: number
}

export interface Marks {
    [
        /**
         * @format uuid
         */
        userID: string
    ]: {
        value: Mark | null,
        /**
         * @minimum 0
         * @maximum 3
         */
        order: number
    }
}

export type BoardSize = BoardPosition
export type Message = EmptyMessage|StartMessage|RejoinMessage|UpdateMessage|DoneMessage|MoveMessage
export type Board = (Mark|null)[][]
export type JoinLobbyMessage = EmptyMessage
export type LeaveLobbyMessage = EmptyMessage

export interface EmptyMessage {}

// Message data sent by server to clients representing a new game round starting.
export interface StartMessage {
    // The assignments of the marks to players for this round.
    marks: Marks
    // Whose turn it is to play.
    mark: Mark
    // The deadline time by which the player must submit their move, or forfeit.
    deadline: number
}

// A game state update sent by the server to rejoined clients.
export interface RejoinMessage {
    // The assignments of the marks to players for this round.
    marks: Marks
    // The current state of the board.
    board: Board
    // Whose turn it is to play.
    mark: Mark
    // The deadline time by which the player must submit their move, or forfeit.
    deadline: number
}

// A game state update sent by the server to clients.
export interface UpdateMessage {
    // The move of current player
    position: BoardPosition | null
    // Who took the move
    marked: Mark
    // Whose turn it is to play.
    mark: Mark
    // The deadline time by which the player must submit their move, or forfeit.
    deadline: number
    // Number of moves taken.
    moveCount: number
}

// Complete game round with winner announcement.
export interface DoneMessage {
    // The move of current player
    position: BoardPosition | null
    // The winner of the game, if any. Unspecified if it's a draw.
    winner: Mark | null
    // Winner board positions, if any. Used to display the row, column, or diagonal that won the game.
    // May be empty if it's a draw or the winner is by forfeit.
    winnerPositions: BoardPosition[] | null
    // Next round start time.
    nextGameStart: number
}

// A player intends to make a move.
export interface MoveMessage {
    // The position the player wants to place their mark in.
    position: BoardPosition;
}
