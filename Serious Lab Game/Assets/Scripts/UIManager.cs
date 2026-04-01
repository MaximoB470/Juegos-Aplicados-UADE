using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;                // ← si usás TextMeshPro; reemplazá por UnityEngine.UI si usás Text legacy

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // ── Canvases ─────────────────────────────────────────────────────────────
    [Header("Canvases")]
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private GameObject winCanvas;
    [SerializeField] private GameObject loseCanvas;

    [Header("Info Panel")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TMP_Text infoPanelTitle;      
    [SerializeField] private TMP_Text infoPanelDescription; 

    [Header("HUD (opcional)")]
    [SerializeField] private TMP_Text progressText; 
    [SerializeField] private TMP_Text timerText;    

    public bool IsPaused { get; private set; }
    private ClickPoint currentPoint; 
    private SceneController GetSceneController() =>
        (SceneController)ServiceLocator.Instance.GetService("SceneController");

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        bool isGameplayScene = SceneManager.GetActiveScene().name != "MenuScene";

        if (!isGameplayScene)
            ShowMenu();
        else
        {
            HideAll();
            ResumeGame();
        }
    }

    public void ShowMenu()
    {
        HideAll();
        SetActive(menuCanvas, true);
        PauseGame();
    }

    public void ShowWin()
    {
        HideAll();
        SetActive(winCanvas, true);
        PauseGame();
    }

    public void ShowLose()
    {
        HideAll();
        SetActive(loseCanvas, true);
        PauseGame();
    }

    /// <summary>
    /// Abre el InfoPanel con el contenido del ClickPoint recibido.
    /// No marca el punto todavía — eso ocurre al cerrarlo.
    /// </summary>
    public void ShowInfoPanel(ClickPoint point)
    {
        if (infoPanel == null) return;

        currentPoint = point;

        if (infoPanelTitle != null)
            infoPanelTitle.text = point.ErrorTitle;

        if (infoPanelDescription != null)
            infoPanelDescription.text = point.ErrorDescription;

        infoPanel.SetActive(true);
        PauseGame();
    }

    /// <summary>
    /// Cerrá el InfoPanel desde el botón "Cerrar" del panel.
    /// Marca el punto como encontrado y notifica al GameManager.
    /// </summary>
    public void CloseInfoPanel()
    {
        if (infoPanel == null) return;

        infoPanel.SetActive(false);
        ResumeGame();

        if (currentPoint != null)
        {
            currentPoint.MarkAsFound();
            GameManager.Instance.RegisterPointFound();
            currentPoint = null;
        }
    }

    public void HideAll()
    {
        SetActive(menuCanvas, false);
        SetActive(winCanvas, false);
        SetActive(loseCanvas, false);
        SetActive(infoPanel, false);
    }

    public void UpdateProgressDisplay(int found, int total)
    {
        if (progressText != null)
            progressText.text = $"{found} / {total} errores encontrados";
    }

    public void UpdateTimerDisplay(float seconds)
    {
        if (timerText == null) return;
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        timerText.text = $"{m}:{s:D2}";
    }

    public void StartGameButton()
    {
        GetSceneController()?.StartGame();
    }

    public void ReloadSceneButton()
    {
        GetSceneController()?.ReloadScene();
    }

    public void LoadMenuButton()
    {
        GetSceneController()?.LoadMenu();
    }

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

    private static void SetActive(GameObject go, bool value)
    {
        if (go != null) go.SetActive(value);
    }

    
}