using UnityEngine;
using UnityEngine.SceneManagement;

public class WinPopup : MonoBehaviour
{
    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
