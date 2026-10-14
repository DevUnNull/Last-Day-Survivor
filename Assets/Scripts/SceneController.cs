
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Chuyển đến Scene theo tên
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Chơi game
    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    // Quay về Main Menu
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // Thoát game
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
