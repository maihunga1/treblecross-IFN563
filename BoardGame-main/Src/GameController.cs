namespace BoardGame;

using System.Linq;
using System.Text.Json;

/// <summary>
/// Controller for the game, which stores game data, handles a player's move, handles undo and redo commands and saves the game.
/// </summary>
public class GameController
{
  // Game identifier value
  public string GameID { get; set; }
  // Player turn indicator. PlayerTurn's value will either be 0 or 1 depending on which player is playing in this turn (the player who takes the first turn will be 0, while the player who takes the 2nd turn will be 1)
  public int PlayerTurn = 0;
  public Board Board { get; set; }
  // List of the game's players
  public Player[] Players = new Player[2];
  // Player that is taking the current turn
  public Player CurrentPlayer { get; set; }
  // Indicator for game mode (true if the game mode is "human versus computer", false if the game mode is "human versus human")
  public bool OpponentIsBot;

  public Record Record;

  public GameController(Player[] players, Board board, int playerTurn, bool opponentIsBot, Record record)
  {
    Players = players;
    Board = board;
    PlayerTurn = playerTurn;
    CurrentPlayer = players[playerTurn];
    OpponentIsBot = opponentIsBot;
    Record = record;
  }

  public GameController(bool opponentIsBot, int boardColumnSizeInput, int boardRowSizeInput = 1)
  {
    GameID = "s1";
    Prompt.Playing("Name of player 1 - enter to set as `player_1`?");
    string Player1IDInput = Console.ReadLine() ?? "";

    Players[0] = new PlayerHuman { ID = Player1IDInput == "" ? "player_1" : Player1IDInput };

    if (!opponentIsBot)
    {
      Prompt.Playing("Name of player 2 - enter to set as `player_2`?");
      string Player2IDInput = Console.ReadLine() ?? "";

      Players[1] = new PlayerHuman { ID = Player2IDInput == "" ? "player_2" : Player2IDInput };
    }
    else
    {
      Players[1] = new PlayerBot { ID = "bot_0" };
    }

    Board = new Board(boardColumnSizeInput, boardRowSizeInput);
    Board.PrintBoard();

    PlayerTurn = 0;
    CurrentPlayer = Players[PlayerTurn];
    OpponentIsBot = opponentIsBot;
    Record = new Record();

    Console.WriteLine("Game start!");
  }

  /// <summary>
  /// Handle a player's move. This handles the "play [x]" command
  /// </summary>
  /// 
  /// <param name="colPosition">
  /// Column position of the play token
  /// </param>
  /// 
  /// <param name="rowPosition">
  /// Row position of the play token
  /// </param>
  /// 
  /// <exception cref="Exception"></exception>
  public GameProgramStatus HandlePlayerMove(int colPosition, int rowPosition = 1)
  {
    //Create a play token based on the column and row parameters, given by the player. This token represents a player's move
    PlayToken token = CurrentPlayer.Play(colPosition);

    //Check if the player's move is valid
    bool isValidMove = ValidateMove(token);

    //Throws invalid move exception if the move is not valid 
    if (!isValidMove)
    {
      throw new Exception("INVALID_MOVE");
    }
    else
    {
      //If the move is valid, place the new token on the game's board
      Board.PlaceToken(token);
      Board.PrintBoard();
      //Add the latest valid move to the game's record
      Record.Update(token);

      //Check if this move satisfies winning condition and makes the current player the winner
      bool hasWon = CheckWin();
      //End the current game if the winning condition is satisfied
      if (hasWon)
      {
        return GameProgramStatus.EndGame;
      }

      //If the game mode is "human versus computer", generate the next move
      if (OpponentIsBot)
      {
        CurrentPlayer = Players[1]; //the bot player

        bool isBotValidMove = false;
        //Ends this loop when the bot/computer makes a valid move on the board
        while (isBotValidMove == false)
        {
          //Generate the bot/computer's move
          PlayToken botToken = Players[1].Play(Board.CurrentBoard[0].Length, Board.CurrentBoard.Length);
          //Check if this move makes the bot/computer the winner or not
          isBotValidMove = ValidateMove(botToken);

          if (isBotValidMove)
          {
            //If the bot/computer's move is valid, place the new token on the game board and update the game's record
            Prompt.Playing($"- {CurrentPlayer.ID} turn", true);
            Board.PlaceToken(botToken);
            Board.PrintBoard();
            Record.Update(botToken);
          }
        };

        //Check if this move satisfies winning condition and makes the bot/computer the winner
        hasWon = CheckWin();
        //End the current game if the winning condition is satisfied
        if (hasWon)
        {
          return GameProgramStatus.EndGame;
        }

        CurrentPlayer = Players[0];
        //put in record
      }
      else
      {
        PlayerTurn = PlayerTurn == 0 ? 1 : 0;
        CurrentPlayer = Players[PlayerTurn];
      }
    }

    return GameProgramStatus.Playing;
  }

  /// <summary>
  /// Check if the player's move is valid or not
  /// </summary>
  /// 
  /// <param name="token">
  /// A token that will be placed on the game board by a player. This will also represents a player's move.
  /// </param>
  public bool ValidateMove(PlayToken token)
  {
    if (Board.IsOutOfBound(token))
    {
      return false;
    }

    // Check if the requested position on the board is occupied
    if (Board.CurrentBoard[token.Row - 1][token.Column - 1] != 0)
    {
      return false;
    }

    return true;
  }
  /// <summary>
  /// An abstract method to check winning condition from the current game state
  /// </summary>
  public virtual bool CheckWin()
  {
    return false;
  }

  /// <summary>
  /// Handles "undo" command from a player 
  /// </summary>
  public void HandleUndo()
  {
    int currentRecordIndex = Record.Iterator;
    //Get the previous move of the current player.
    int lastStateIndex = Record.GetPrevState(PlayerTurn);

    for (int head = currentRecordIndex - 1; head >= lastStateIndex; head--)
    {
      PlayToken tokenToRemove = Record.Actions[head];
      //Remove the token from the current board to set it to the current "undo" state
      Board.RemoveToken(tokenToRemove);
    }
    Board.PrintBoard();
  }

  /// <summary>
  /// Handles "redo" command from a player 
  /// </summary>
  public void HandleRedo()
  {
    int currentRecordIndex = Record.Iterator;
    // Get the next move of the current player.
    int nextStateIndex = Record.GetNextState();

    for (int head = currentRecordIndex; head < nextStateIndex; head++)
    {
      PlayToken tokenToPlace = Record.Actions[head];
      //Add the token from the current board to set it to the current "redo" state
      Board.PlaceToken(tokenToPlace);
    }
    Board.PrintBoard();
  }

  /// <summary>
  /// Saves the current game to a JSON file, which can be used to restore the current game
  /// </summary>
  public void SaveGame()
  {
    string filePath = "data.json";
    GameSnapshot snapshot = new()
    {
      PlayerIDs = Players.Select((player) => player.ID).ToArray(),
      PlayerTurn = PlayerTurn,
      GameMode = OpponentIsBot ? 1 : 2,
      Board = Board.CurrentBoard,
      Actions = Record.Actions
    };

    string jsonData = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });

    File.WriteAllText(filePath, jsonData);

    Console.WriteLine("Saved game snapshot as data.json");
  }
}
