using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Explicit file-triggered build in an already open editor; never builds on import alone.
[InitializeOnLoad]
public static class PokerPwaBuildRequest
{
    private const string Request = "Temp/PokerPwaBuild.request";
    private const string Result = "Logs/PokerPwaBuild.result";
    private static bool running;
    static PokerPwaBuildRequest() => EditorApplication.update += Poll;
    private static void Poll()
    {
        if (running || EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(Request)) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.isPlaying = false;
            return;
        }
        running = true;
        File.Delete(Request);
        Directory.CreateDirectory("Logs");
        File.WriteAllText(Result, "BUILDING " + DateTime.UtcNow.ToString("O"));
        try
        {
            if (!EditorSceneManager.SaveOpenScenes()) throw new Exception("Cannot save open scenes before build.");
            AssetDatabase.SaveAssets();
            BuildPokerPwa.Build();
            File.WriteAllText(Result, "SUCCEEDED " + DateTime.UtcNow.ToString("O"));
        }
        catch (Exception exception)
        {
            File.WriteAllText(Result, "FAILED " + exception);
            Debug.LogException(exception);
        }
        finally { running = false; }
    }
}
