using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeSelectUI : MonoBehaviour
{
    public const string SelectedGameModeKey = "selectedGameMode";

    public const string BeginnerModeName = "Początkujący";
    public const string ClassicModeName = "Klasyczny";
    public const string FastModeName = "Przyśpieszony";
    public const string Mode420Name = "420";

    public void SelectBeginnerMode()
    {
        SelectModeAndGoToCreateRoom(BeginnerModeName);
    }

    public void SelectClassicMode()
    {
        SelectModeAndGoToCreateRoom(ClassicModeName);
    }

    public void SelectFastMode()
    {
        SelectModeAndGoToCreateRoom(FastModeName);
    }

    public void Select420Mode()
    {
        SelectModeAndGoToCreateRoom(Mode420Name);
    }

    public void GoBack()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public static string GetSelectedGameMode()
    {
        return PlayerPrefs.GetString(SelectedGameModeKey, ClassicModeName);
    }

    private void SelectModeAndGoToCreateRoom(string modeName)
    {
        PlayerPrefs.SetString(SelectedGameModeKey, modeName);
        PlayerPrefs.Save();

        SceneManager.LoadScene("CreateRoom");
    }
}