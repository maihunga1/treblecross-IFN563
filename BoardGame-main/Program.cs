using System;
using System.Text.Json;

namespace BoardGame
{

    //command list
    //available to all: help
    //game: start, load
    //saved files: load game
    //player: undo, redo, play, help, save
    public enum GameProgramStatus
    {
        MainMenu,
        SetupGame,
        Playing,
        //Paused,
        //Continue,
        SavedFiles,
        Help,
        Exit,
        EndGame,
    }

    class Program
    {
        // public const string hasComputer = "true";

        static void Main(string[] _)
        {
            GameProgram game = new();

            game.RunApp();
        } 
    }
}