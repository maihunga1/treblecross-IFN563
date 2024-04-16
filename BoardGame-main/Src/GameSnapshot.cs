namespace BoardGame;

public struct GameSnapshot(
  string[] PlayerIDs,
  int PlayerTurn,
  int GameMode,
  int[][] Board,
  List<PlayToken> Actions
  )
{
  public string[] PlayerIDs { get; set; } = PlayerIDs;
  public int PlayerTurn { get; set; } = PlayerTurn;
  public int GameMode { get; set; } = GameMode;
  public int[][] Board { get; set; } = Board;
  public List<PlayToken> Actions { get; set; } = Actions;
}