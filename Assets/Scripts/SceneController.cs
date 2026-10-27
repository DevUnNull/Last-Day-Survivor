
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField] private MapSelectionController mapSelectionController;

    // Chuyển đến Scene theo tên
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Chơi game: mở Map Selection thay vì vào game trực tiếp
    public void PlayGame()
    {
        if (mapSelectionController != null)
        {
            mapSelectionController.OpenMapSelection();
        }
        else
        {
            var msc = FindFirstObjectByType<MapSelectionController>();
            if (msc != null)
            {
                msc.OpenMapSelection();
            }
            else
            {
                SceneManager.LoadScene("2_InGame");
            }
        }
    }

    // Quay về Main Menu
    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("1_1_MainMenu");
    }

    // Thoát game
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
