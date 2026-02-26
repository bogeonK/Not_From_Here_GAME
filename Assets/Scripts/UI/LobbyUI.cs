using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    public void OnClickStart()
    {
        PlayerPrefs.SetString("Spawn", "Default");
        SceneManager.LoadScene("InGameScene");
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}