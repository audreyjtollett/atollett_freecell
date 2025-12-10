using UnityEngine;

public class PlayGameHandler : MonoBehaviour
{
    public MainMenuController mainMenuController;
    public void HandlePlayGame()
    {
        mainMenuController.StartGame();
    }
}
