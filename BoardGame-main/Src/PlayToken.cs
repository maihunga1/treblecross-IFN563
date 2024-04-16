namespace BoardGame;

/// <summary>
/// Defines a token that will be placed on the game board by a player. This will also represents a player's move.
/// </summary>
/// 
/// <param name="row">
/// Row position of the play token
/// </param>
/// 
/// <param name="column">
/// Column position of the play token
/// </param>
/// 
/// <param name="playerID">
/// Player's identifier
/// </param>
public struct PlayToken(int row, int column, string playerID)
{
    public int Row { get; set; } = row;
    public int Column { get; set; } = column;
    public string PlayerID { get; set; } = playerID;
}