using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DataImporter : MonoBehaviour
{
#if UNITY_EDITOR
    // Google Sheets ID
    private const string GOOGLE_SHEETS_ID = "1yRppgOM2Mgv7oh_b4S8wdnxVLZ0yx3HgWl51iP8_wRg";
    
    // Map of CSV file names to their sheet GIDs (sheet IDs)
    private static readonly Dictionary<string, string> sheetGids = new Dictionary<string, string>
    {
        { "Depths - Card Game - Equipment.csv", "1692924861" },
        { "Depths - Card Game - Mines.csv", "1518700764" },
        { "Depths - Card Game - Monsters.csv", "175727478" },
        { "Depths - Card Game - Resources.csv", "120251468" },
        { "Depths - Card Game - Tiles.csv", "1079755230" },
        { "Depths - Card Game - Utility.csv", "696885727" }
    };

    [MenuItem("Tools/Import CSV Data")]
    public static void ImportCSVData()
    {
        EditorUtility.DisplayProgressBar("Importing CSV Data", "Starting download...", 0f);
        
        string dataPath = Path.Combine(Application.dataPath, "Resources", "Data");
        
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }

        int fileIndex = 0;
        foreach (var kvp in sheetGids)
        {
            string fileName = kvp.Key;
            string gid = kvp.Value;
            float progress = (float)fileIndex / sheetGids.Count;
            
            EditorUtility.DisplayProgressBar("Importing CSV Data", $"Downloading {fileName}...", progress);
            
            // Build Google Sheets export URL with GID
            string url = $"https://docs.google.com/spreadsheets/d/{GOOGLE_SHEETS_ID}/export?format=csv&gid={gid}";
            string destinationPath = Path.Combine(dataPath, fileName);
            
            try
            {
                // Using UnityWebRequest for downloading
                var request = UnityWebRequest.Get(url);
                var operation = request.SendWebRequest();
                
                // Wait for download to complete
                while (!operation.isDone)
                {
                    System.Threading.Thread.Sleep(10);
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    File.WriteAllBytes(destinationPath, request.downloadHandler.data);
                    Debug.Log($"Downloaded: {fileName}");
                }
                else
                {
                    Debug.LogError($"Failed to download {fileName}: {request.error}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error downloading {fileName}: {e.Message}");
            }
            
            fileIndex++;
        }
        
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }
#endif
}
