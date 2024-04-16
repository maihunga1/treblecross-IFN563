namespace BoardGame;
/// <summary>
/// Help system to assist users with the available commands
/// </summary>
public class Help
{
  public static void PrintHelp(GameProgramStatus state)
  {
    if (state == GameProgramStatus.Playing)
    {
      PrintGameHelp();
    }
    else
    {
      PrintMenuHelp();
    }
  }

  public static void PrintGameHelp()
  {
    Console.WriteLine("[GAME]");
    Console.WriteLine("----------------");
    Console.WriteLine("play [position] - Put a new piece/token on the board. E.g. play 5");
    Console.WriteLine("undo - Revert to the last state of the board");
    Console.WriteLine("redo - Display the next state of the board");
    Console.WriteLine("save - Save current game");
    Console.WriteLine("quit - Go back to main menu");
    Console.WriteLine("----------------");
  }

  public static void PrintMenuHelp()
  {
    //start, load, exit
    Console.WriteLine("[MAIN MENU]");
    Console.WriteLine("----------------");
    Console.WriteLine("start - Start new game (Treblecross)");
    Console.WriteLine("load - Continue an existing game");
    Console.WriteLine("exit - Exit program");
    Console.WriteLine("----------------");
  }
}
