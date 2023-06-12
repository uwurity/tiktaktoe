namespace tiktaktoe.Messages;

public enum OpCode : long
{
    // Leave the lobby.
    LeaveLobby = 0,

    // Join the lobby.
    JoinLobby = 1,

    // New game round starting.
    Start = 2,

    // User is rejoining the game.
    Rejoin = 3,

    // Update to the state of an ongoing round.
    Update = 4,

    // Update to the game label.
    Label = 5,

    // A game round has just completed.
    Done = 6,

    // A move the player wishes to make and sends to the server.
    Move = 7,

    // Move was rejected.
    Rejected = 8,

    // Player is ready.
    Ready = 9,
}