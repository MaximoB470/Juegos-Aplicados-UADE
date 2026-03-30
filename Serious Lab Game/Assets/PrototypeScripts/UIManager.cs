using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Canvases (opcionales según escena)")]
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private GameObject winCanvas;
    [SerializeField] private GameObject loseCanvas;

    [Header("Info Panel")]
    [SerializeField] private GameObject infoPanel;

    public bool IsPaused { get; private set; }

    private bool isGameplayScene;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        isGameplayScene = SceneManager.GetActiveScene().name == "Game";

        if (!isGameplayScene)
        {
            ShowMenu();
        }
        else
        {
            HideAll();
            ResumeGame();
        }
    }

    // ---------------- UI STATES ----------------

    public void ShowMenu()
    {
        HideAll();

        if (menuCanvas != null)
            menuCanvas.SetActive(true);

        PauseGame();
    }

    public void ShowWin()
    {
        HideAll();

        if (winCanvas != null)
            winCanvas.SetActive(true);

        PauseGame();
    }

    public void ShowLose()
    {
        HideAll();

        if (loseCanvas != null)
            loseCanvas.SetActive(true);

        PauseGame();
    }

    public void ShowInfoPanel()
    {
        if (infoPanel == null) return;

        infoPanel.SetActive(true);
        PauseGame();
    }

    public void CloseInfoPanel()
    {
        if (infoPanel == null) return;

        infoPanel.SetActive(false);
        ResumeGame();
    }

    public void HideAll()
    {
        if (menuCanvas != null) menuCanvas.SetActive(false);
        if (winCanvas != null) winCanvas.SetActive(false);
        if (loseCanvas != null) loseCanvas.SetActive(false);
        if (infoPanel != null) infoPanel.SetActive(false);
    }

    // ---------------- BOTONES ----------------

    public void StartGameButton()
    {
        var sceneController = (SceneController)ServiceLocator.Instance.GetService("SceneController");
        if (sceneController != null)
        {
            sceneController.StartGame();
        }
    }

    public void ReloadSceneButton()
    {
        var sceneController = (SceneController)ServiceLocator.Instance.GetService("SceneController");
        if (sceneController != null)
        {
            sceneController.ReloadScene();
        }
    }

    public void LoadMenuButton()
    {
        var sceneController = (SceneController)ServiceLocator.Instance.GetService("SceneController");
        if (sceneController != null)
        {
            sceneController.LoadMenu();
        }
    }

    // ---------------- GAME CONTROL ----------------

    public void PauseGame()
    {
        Time.timeScale = 0f;
        IsPaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        IsPaused = false;
    }
}