namespace BoardGame;

/// <summary>
/// Record of a game, which includes actions made by players and an iterator for iterating through the game's state
/// </summary>
public class Record
{
  /// <summary>
  /// Stores players' moves
  /// </summary>
  public List<PlayToken> Actions { get; set; }
  public int Iterator;

  public Record()
  {
    Actions = [];
    Iterator = 0;
  }

  /// <summary>
  /// Get the previous move of the current player. This method handles "undo" command for the game.
  /// Example: Player 1 and player 2 makes the moves A, B, C, D in turns consecutively. Player 1 commands "undo".  This method then will returns move C.
  /// </summary>
  /// <param name="currentPlayerTurn">
  /// Player turn indicator. PlayerTurn's value will either be 0 or 1 depending on which player is playing in this turn (the player who takes the first turn will be 0, while the player who takes the 2nd turn will be 1)
  /// </param>
  /// <exception cref="Exception">
  /// If the `Iterator` reachs the top of the `Actions` list, it cannot trace back the game record any further.
  /// </exception>
  public int GetPrevState(int currentPlayerTurn)
  {
    if (Iterator == currentPlayerTurn)
    {
      throw new Exception("Cannot undo further.");
    }

    Iterator = Math.Max(Iterator - 2, 0);
    return Iterator;
  }

  /// <summary>
  /// Get the next move of the current player. This method handles "redo" command for the game.
  /// Example: Player 1 and player 2 makes the moves A, B, C, D in turns consecutively. Player 1 commands "undo" to get back to "move A" state of the game, then Player 1 commands "redo". This method then will return "move C" state of the game
  /// </summary>
  /// <param name="currentPlayerTurn">
  /// Player turn indicator. PlayerTurn's value will either be 0 or 1 depending on which player is playing in this turn (the player who takes the first turn will be 0, while the player who takes the 2nd turn will be 1)
  /// </param>
  /// <exception cref="Exception">
  /// If the `Iterator` reachs the end of the `Actions` list, it cannot trace back the game record any further.
  /// </exception>
  public int GetNextState()
  {
    if (Iterator == Actions.Count)
    {
      throw new Exception("Cannot redo - this is the latest game state.");
    }

    Iterator = Math.Min(Iterator + 2, Actions.Count);
    return Iterator;
  }

  /// <summary>
  /// Store the latest move made by a player to the `Actions` list
  /// </summary>
  /// <param name="token">A move made by a player (represents by a valid play token that is just placed onto the game board)</param>
  public void Update(PlayToken token)
  {
    if (Iterator < Actions.Count)
    {
      Actions = Actions[..Iterator];
    }

    Actions.Add(token);
    Iterator = Actions.Count;
  }
}
