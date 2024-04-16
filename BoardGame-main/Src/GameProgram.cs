using System.Text.Json;
using System.Text.RegularExpressions;
namespace BoardGame;

public class GameProgram
{
  //Controller for the game, which stores game data, handles a player's move, handles undo and redo commands and saves the game.
  public GameController? GameController;

  //Stores the program's state
  public GameProgramStatus GameState { get; set; }

  public GameProgram()
  {
    GameState = GameProgramStatus.MainMenu;
  }

  /// <summary>
  /// Handles the user commands, when the user is at "main menu screen" (GameState = GameProgramStatus.MainMenu)
  /// </summary>
  void HandleMenuCommand(string command)
  {
    switch (command)
    {
      case "start":
        //Starts a new game
        SetupGame();
        break;

      case "load":
        //Loads a game from a save file
        LoadGame();
        break;

      case "exit":
        //Exit the program
        GameState = GameProgramStatus.Exit;
        break;

      case "help":
        //Print a lists of valid commands for the "main menu screen"
        Help.PrintHelp(GameProgramStatus.MainMenu);
        break;

      default:
        //Throws exception if the command input is invalid
        throw new Exception("INVALID");
    }
  }

  /// <summary>
  /// Handles the user commands, when the user is playing a game (GameState = GameProgramStatus.Playing)
  /// </summary>
  void HandlePlayCommand(string command)
  {
    //Checks if there's an active game. If there's not any active game, throws an exception
    if (GameController == null)
    {
      throw new Exception("INVALID_STATE");
    }

    switch (command)
    {
      case "save":
        //Saves the current game
        GameController.SaveGame();
        break;

      case "undo":
        //Undo a move
        GameController.HandleUndo();
        break;

      case "redo":
        //Redo a move
        GameController.HandleRedo();
        break;

      case "quit":
        // Quit game to main menu - assume no auto save needed
        GameState = GameProgramStatus.MainMenu;
        break;

      case "help":
        //Print a lists of valid commands for the game
        Help.PrintHelp(GameProgramStatus.Playing);
        break;

      default:
        //Handles other commands
        //Checks if the command is a "play" command
        if (command.Contains("play"))
        {
          if (Regex.IsMatch(command, @"^play ([0-9]+)"))
          {
            string tokenIndex = command.Split(" ")[1];
            if (int.TryParse(tokenIndex, out int position))
            {
              //If it is a valid "play" command, player should be able to make a move from the command input
              GameState = GameController.HandlePlayerMove(position);
            }
            else
            {
              //Throws exception if the command input is invalid
              Console.WriteLine($"Cannot parse command position `{tokenIndex}`.");
              throw new Exception("INVALID_MOVE");
            }
            break;
          }
          //Throws exception if the command input is invalid
          throw new Exception("INVALID_MOVE");
        }
        //Throws exception if the command input is invalid
        throw new Exception("INVALID");
    }
  }

  /// <summary>
  /// Runs a list of prompts to get the user's input for initiating a new game, then initialise a new game.
  /// </summary>
  void SetupGame()
  {
    try
    {
      GameState = GameProgramStatus.SetupGame;

      //Select the game mode ("human versus human" or human versus computer"). If user doesn't enter any input, the game mode is set to 1 by default
      Prompt.Setup("Select mode (1 - default) vs computer or (2) vs human - enter to set default mode 1?");
      string gameMode = Console.ReadLine()?.Trim() ?? "";
      if (gameMode == "")
      {
        gameMode = "1";
      }
      else if (gameMode != "1" && gameMode != "2")
      {
        throw new Exception($"Invalid game mode `{gameMode}`.");
      }

      //Prompt user to get the board size. If user doesn't enter any input, the game board size is set to 10 by default
      Prompt.Setup("Board size - enter to set default size 10?");
      string? boardSizeInput = Console.ReadLine() ?? "";
      if (boardSizeInput == "")
      {
        boardSizeInput = "10";
      }

      if (!int.TryParse(boardSizeInput, out int boardSize))
      {
        throw new Exception($"Cannot parse board size `{boardSizeInput}`.");
      }

      //Create a new game by setting up a game controller
      GameController = new TrebleCrossController(gameMode == "1", boardSize);
      //Set the program state `GameState` to `Playing` state
      GameState = GameProgramStatus.Playing;
    }
    catch (Exception error)
    {
      Console.WriteLine($"{error.Message}");
      Console.WriteLine("Back to main menu.");
      GameState = GameProgramStatus.MainMenu;
    }
  }

  /// <summary>
  /// Loads game from a saved file and set up a games from the saved file data.
  /// </summary>
  void LoadGame()
  {
    string filePath = "data.json";

    //Checks if the file exists
    if (File.Exists(filePath))
    {
      try
      {
        //Parsing the data from the save file
        string jsonReadData = File.ReadAllText(filePath);
        Console.WriteLine(jsonReadData);
        GameSnapshot snapshot = JsonSerializer.Deserialize<GameSnapshot>(jsonReadData);

        Player[] players =
        [
            new PlayerHuman { ID = snapshot.PlayerIDs[0] },
            snapshot.PlayerIDs[1] == "bot_0" || snapshot.PlayerIDs[1] == null ? new PlayerBot() { ID = "bot_0" } : new PlayerHuman() { ID = snapshot.PlayerIDs[1] },
        ];
        Board board = new()
        {
          CurrentBoard = snapshot.Board
        };

        int playerTurn = snapshot.PlayerTurn;

        Record record = new()
        {
          Actions = snapshot.Actions,
          Iterator = 0
        };

        //Create a new game by setting up a game controller
        GameController = new TrebleCrossController(players, board, playerTurn, snapshot.GameMode == 1, record);

        //Restore the game board
        foreach (var action in snapshot.Actions)
        {
          Console.WriteLine(action.PlayerID);
          PlayToken token = new(action.Row, action.Column, action.PlayerID);
          board.PlaceToken(token);
          board.PrintBoard();

          if (GameController.CheckWin())
          {
            GameState = GameProgramStatus.EndGame;
            Prompt.Playing($"{GameController.CurrentPlayer.ID} won!", true); // Announce winner
            break; // Exit the loop if someone wins
          }

        }

        if (GameState != GameProgramStatus.EndGame && GameController.CheckWin())
        {
          GameState = GameProgramStatus.EndGame;
          Prompt.Playing("It's a Draw!", true); // Announce draw
        }
        else
        {
          GameState = GameProgramStatus.Playing;  // Set Playing state if no winner/draw
        }

        GameState = GameProgramStatus.Playing;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error loading game data: {ex.Message}");
        Console.WriteLine("Back to main menu.");
        GameState = GameProgramStatus.MainMenu;
      }
    }
    else
    {
      Console.WriteLine("File not found: data.json");
      Console.WriteLine("Back to main menu.");
      GameState = GameProgramStatus.MainMenu;
    }
  }

  /// <summary>
  /// Runs the game program, which is a loop that runs infinitely until the program state is set to `Exit` (GameState = GameProgramStatus.Exit). Prompts the user and handles the user command based on the current program state.
  /// </summary>
  public void RunApp()
  {
    while (true)
    {
      string? command = null;
      try
      {
        //Breaks the loop and end the program if the program state is `Exit`
        if (GameState == GameProgramStatus.Exit)
        {
          break;
        }
        //Handles the user commands, when the user is at "main menu screen"
        else if (GameState == GameProgramStatus.MainMenu)
        {
          Prompt.MainMenu("Type `help` to get a list of command");

          command = Console.ReadLine() ?? "";
          HandleMenuCommand(command.Trim());
          continue;
        }
        //Handles the user commands, when the user is playing a game
        else if (GameState == GameProgramStatus.Playing)
        {
          if (GameController == null)
          {
            throw new Exception("INVALID_STATE");
          }

          string currentPlayerID = GameController.CurrentPlayer.ID;
          Prompt.Playing($"- {currentPlayerID} turn");

          command = Console.ReadLine() ?? "";
          HandlePlayCommand(command.Trim());
          continue;
        }
        //Handles the scenario when game ends and user is sent back to the main menu
        else if (GameState == GameProgramStatus.EndGame)
        {
          if (GameController == null)
          {
            throw new Exception("INVALID_STATE");
          }

          Prompt.Playing($"{GameController.CurrentPlayer.ID} won!", true);

          GameController = null;
          GameState = GameProgramStatus.MainMenu;
        }
        else
        {
          throw new Exception("INVALID_STATE");
        }

      }
      //Catches all the exceptions in the program
      catch (Exception error)
      {
        switch (error.Message)
        {
          case "INVALID_MOVE":
            //Handles invalid player's move
            Console.WriteLine($"`{command}` is not a valid move. To place a token on the board, type \"play [position]\". Alternatively, type \"help\" to see a list of command.");
            GameState = GameProgramStatus.Playing;
            break;
          case "INVALID_STATE":
            //Handles invalid state of the program - return to main menu
            Console.WriteLine("Invalid state of app. Return to Main Menu.");
            break;
          case "INVALID":
            //Handles all other invalid commands
            Console.WriteLine($"`{command}` is not a valid command. Type \"help\" to see a list of valid command.");
            break;
          default:
            //Handles all other errors
            Console.WriteLine(error.Message);
            Console.WriteLine($"Unable to process `{command}`. Type \"help\" to see a list of valid command.");
            break;
        }
      }
    }
  }
}