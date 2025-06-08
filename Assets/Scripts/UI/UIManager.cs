using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    public GameObject GameOverPanel;
    public GameObject PausePanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Ensure panels are hidden at the start
        if (GameOverPanel != null) GameOverPanel.SetActive(false);
        if (PausePanel != null) PausePanel.SetActive(false);
    }

    public void ShowPauseMenu()
    {
        PausePanel.SetActive(true);
    }

    public void HidePauseMenu()
    {
        PausePanel.SetActive(false);
    }

    public void ShowGameOverMenu()
    {
        GameOverPanel.SetActive(true);
    }

    public void HideGameOverMenu()
    {
        GameOverPanel.SetActive(false);
    }

    public void OnRestartLevelButton()
    {
        HideGameOverMenu();
        GameManager.Instance.RestartLevel();
    }

    public void OnMainMenuButton()
    {
        HidePauseMenu();
        GameManager.Instance.ReturnToMainMenu();
    }

    public void OnFinishGameButton()
    {
        HidePauseMenu();
        GameManager.Instance.StartNewLevel();
    }
}
