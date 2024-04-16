namespace BoardGame;

public class Utils
{
  /// <summary>
  /// Prints out the current program's screen (program's state)
  /// </summary>
  public static string GetScreen(GameProgramStatus state)
  {
    switch (state)
    {
      case GameProgramStatus.MainMenu:
        return "MAIN MENU";
      case GameProgramStatus.Playing:
        return "GAME";
      case GameProgramStatus.SavedFiles:
        return "SAVED FILES";
      case GameProgramStatus.SetupGame:
        return "NEW GAME";
      default:
        return "X";
    }
  }
}
