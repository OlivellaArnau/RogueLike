using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public GameObject PausePanel;

    [Header("Buttons")]
    public Button ResumeButton;
    public Button FinishGameButton;
    public Button QuitToDesktopButton;

    private void Start()
    {
        // Add listeners to buttons
        ResumeButton.onClick.AddListener(ResumeGame);
        FinishGameButton.onClick.AddListener(FinishGame);
        QuitToDesktopButton.onClick.AddListener(QuitToDesktop);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.IsPaused == true)
                ResumeGame();
            else
                PauseGame();
        }
    }

    private void PauseGame()
    {
        GameManager.Instance.PauseGame();
    }

    private void ResumeGame()
    {
        GameManager.Instance.ResumeGame();
    }
    private void FinishGame()
    {
        GameManager.Instance.ReturnToMainMenu();
    }
    private void QuitToDesktop()
    {
        Application.Quit();
        Debug.Log("Quit to Desktop.");
    }
}
