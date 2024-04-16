using System.Text.Json;

namespace BoardGame
{
    public class History
    {
        public void SaveGame(GameSnapshot snapshot)
        {
            string filePath = "data.json";

            string jsonData = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(filePath, jsonData);

            Console.WriteLine("Saved game snapshot as data.json");
        }
    }
}
