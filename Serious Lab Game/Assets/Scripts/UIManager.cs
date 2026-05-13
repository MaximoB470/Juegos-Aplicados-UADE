using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Canvases")]
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private GameObject winCanvas;
    [SerializeField] private GameObject loseCanvas;

    [Header("Info Panel")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TMP_Text infoPanelTitle;
    [SerializeField] private TMP_Text infoPanelDescription;

    [Header("HUD")]
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text timerText;

    [Header("Click Counter")]
    [Tooltip("Texto que muestra los clicks incorrectos usados / máximo.")]
    [SerializeField] private TMP_Text clickCounterText;

    [Header("Hint Button")]
    [SerializeField] private Button hintButton;
    [Tooltip("Color del botón cuando quedan pistas disponibles.")]
    [SerializeField] private Color hintAvailableColor = new Color(0.2f, 0.7f, 1f);
    [Tooltip("Color del botón cuando se agotaron todas las pistas.")]
    [SerializeField] private Color hintExhaustedColor = new Color(0.4f, 0.4f, 0.4f);

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

    // ─── Pantallas principales ───────────────────────────────────────────────

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

    // ─── Info / Hint Panel ───────────────────────────────────────────────────

    public void ShowInfoPanel(ClickPoint point)
    {
        if (infoPanel == null) return;

        currentPoint = point;

        if (infoPanelTitle != null) infoPanelTitle.text = point.ErrorTitle;
        if (infoPanelDescription != null) infoPanelDescription.text = point.ErrorDescription;

        infoPanel.SetActive(true);
        PauseGame();
    }

    public void ShowHintPanel(ClickPoint point)
    {
        if (infoPanel == null) return;

        currentPoint = null;

        if (infoPanelTitle != null) infoPanelTitle.text = "Pista";
        if (infoPanelDescription != null) infoPanelDescription.text = point.HintText;

        infoPanel.SetActive(true);
        PauseGame();
    }

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

    // ─── HUD Updates ─────────────────────────────────────────────────────────

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

    /// <summary>
    /// Actualiza el texto del contador de clicks incorrectos.
    /// Si maxClicks es 0 (sin límite) oculta el texto.
    /// </summary>
    public void UpdateClickDisplay(int wrongClicks, int maxClicks)
    {
        if (clickCounterText == null) return;

        if (maxClicks <= 0)
        {
            clickCounterText.gameObject.SetActive(false);
            return;
        }

        clickCounterText.gameObject.SetActive(true);
        clickCounterText.text = $"Clicks: {wrongClicks} / {maxClicks}";
    }

    /// <summary>
    /// Actualiza el color y el estado interactivo del botón de pistas.
    /// </summary>
    public void UpdateHintButton(int hintsUsed, int maxHints)
    {
        if (hintButton == null) return;

        bool exhausted = hintsUsed >= maxHints;

        // Bloquear interacción
        hintButton.interactable = !exhausted;

        // Cambiar color de la imagen del botón
        var img = hintButton.GetComponent<Image>();
        if (img != null)
            img.color = exhausted ? hintExhaustedColor : hintAvailableColor;

        // Cambiar texto del botón si tiene un TMP_Text hijo
        var label = hintButton.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = exhausted
                ? "Sin pistas"
                : $"Pista ({maxHints - hintsUsed} restantes)";
        }
    }

    // ─── Botones ─────────────────────────────────────────────────────────────

    public void StartGameButton() => GetSceneController()?.StartGame();
    public void HintButton() => GameManager.Instance?.ShowHint();
    public void ReloadSceneButton() => GetSceneController()?.ReloadScene();
    public void LoadMenuButton() => GetSceneController()?.LoadMenu();
    public void LoadLevelTwo() => GetSceneController()?.LoadSecondLevel();

    // ─── Pause / Resume ──────────────────────────────────────────────────────

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

    // ─── Util ────────────────────────────────────────────────────────────────

    private static void SetActive(GameObject go, bool value)
    {
        if (go != null) go.SetActive(value);
    }
}