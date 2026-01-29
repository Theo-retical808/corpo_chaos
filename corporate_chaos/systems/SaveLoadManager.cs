using System.IO;
using System.Text.Json;
using CorporateChaos.Models;

namespace CorporateChaos.Systems
{
    public class SaveLoadManager
    {
        private const string SAVE_FOLDER = "sv_game";
        private const string RUNS_FOLDER = "game_runs";
        private const string RUNS_FILE = "runs_history.json";

        public SaveLoadManager()
        {
            EnsureDirectoriesExist();
        }

        private void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(SAVE_FOLDER))
                Directory.CreateDirectory(SAVE_FOLDER);
            
            if (!Directory.Exists(RUNS_FOLDER))
                Directory.CreateDirectory(RUNS_FOLDER);
        }

        public bool SaveGame(GameSave gameSave)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string fileName = Path.Combine(SAVE_FOLDER, gameSave.GetFileName());
                string json = JsonSerializer.Serialize(gameSave, options);
                File.WriteAllText(fileName, json);
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving game: {ex.Message}");
                return false;
            }
        }

        public GameSave? LoadGame(string fileName)
        {
            try
            {
                string filePath = Path.Combine(SAVE_FOLDER, fileName);
                if (!File.Exists(filePath))
                    return null;

                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<GameSave>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading game: {ex.Message}");
                return null;
            }
        }

        public List<string> GetSaveFiles()
        {
            try
            {
                return Directory.GetFiles(SAVE_FOLDER, "*.json")
                    .Select(Path.GetFileName)
                    .Where(f => f != null)
                    .Cast<string>()
                    .OrderByDescending(f => File.GetLastWriteTime(Path.Combine(SAVE_FOLDER, f)))
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public bool DeleteSave(string fileName)
        {
            try
            {
                string filePath = Path.Combine(SAVE_FOLDER, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public void SaveGameRun(GameRunRecord runRecord)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                // Save individual run file
                string runFileName = Path.Combine(RUNS_FOLDER, $"run_{runRecord.RunId}.json");
                string runJson = JsonSerializer.Serialize(runRecord, options);
                File.WriteAllText(runFileName, runJson);

                // Update runs history
                var runsHistory = LoadRunsHistory();
                runsHistory.Add(runRecord);
                
                // Keep only last 50 runs
                runsHistory = runsHistory.OrderByDescending(r => r.StartDate)
                    .Take(50)
                    .ToList();

                string historyJson = JsonSerializer.Serialize(runsHistory, options);
                File.WriteAllText(RUNS_FILE, historyJson);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving game run: {ex.Message}");
            }
        }

        public List<GameRunRecord> LoadRunsHistory()
        {
            try
            {
                if (File.Exists(RUNS_FILE))
                {
                    string json = File.ReadAllText(RUNS_FILE);
                    return JsonSerializer.Deserialize<List<GameRunRecord>>(json) ?? new List<GameRunRecord>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading runs history: {ex.Message}");
            }

            return new List<GameRunRecord>();
        }

        public GameRunRecord? LoadGameRun(string runId)
        {
            try
            {
                string runFileName = Path.Combine(RUNS_FOLDER, $"run_{runId}.json");
                if (File.Exists(runFileName))
                {
                    string json = File.ReadAllText(runFileName);
                    return JsonSerializer.Deserialize<GameRunRecord>(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading game run: {ex.Message}");
            }

            return null;
        }
    }
}