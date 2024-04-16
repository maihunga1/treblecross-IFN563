namespace BoardGame;

/// <summary>
/// Game controller for Treble Cross game
/// </summary>
public class TrebleCrossController : GameController
{
  public TrebleCrossController(Player[] players, Board board, int playerTurn, bool opponentIsBot, Record record)
    : base(players, board, playerTurn, opponentIsBot, record)
  {
  }

  public TrebleCrossController(bool opponentIsBot, int boardSize)
    : base(opponentIsBot, boardSize)
  {
  }

  /// <summary>
  /// A method to check winning condition from the current game state
  /// </summary>
  public override bool CheckWin()
  {
    int countSequence = 0;

    for (int col = 0; col < Board.CurrentBoard[0].Length; col++)
    {
      if (Board.CurrentBoard[0][col] == 1)
      {
        countSequence++;
        if (countSequence == 3)
        {
          return true;
        }
      }
      else
      {
        countSequence = 0;
      }
    }
    return false;
  }
}