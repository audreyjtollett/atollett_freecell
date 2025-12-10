using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject instructionsPanel;
    public void StartGame()
    {
        SceneManager.LoadScene("Solitaire", LoadSceneMode.Single);
    }

    public void ExitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    public void ShowInstructions()
    {
        instructionsPanel.SetActive(true);
    }
}
