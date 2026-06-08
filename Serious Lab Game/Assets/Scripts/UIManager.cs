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

    [Header("Timer")]
 
    [SerializeField] private Image timerFillImage;

    [Header("Click Counter")]
    [Tooltip("Texto que muestra los clicks incorrectos usados / máximo.")]
    [SerializeField] private TMP_Text clickCounterText;

    [Header("Hint Button")]
    [SerializeField] private Button hintButton;
    [SerializeField] private Color hintAvailableColor = new Color(0.2f, 0.7f, 1f);
    [SerializeField] private Color hintExhaustedColor = new Color(0.4f, 0.4f, 0.4f);

    [Header("Hints Display")]
    [SerializeField] private TMP_Text hintsRemainingText;
    [SerializeField] private Image[] hintIcons;
    [SerializeField] private Color hintIconAvailableColor = Color.white;
    [SerializeField] private Color hintIconSpentColor = Color.gray;

    [Header("Win Screen")]
    [SerializeField] private TMP_Text winScoreText;

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

    // ─────────────────────────────────────────────
    // Pantallas principales
    // ─────────────────────────────────────────────

    public void ShowMenu()
    {
        HideAll();
        SetActive(menuCanvas, true);
        PauseGame();
    }

    public void ShowWin(float score = -1f)
    {
        HideAll();
        SetActive(winCanvas, true);
        PauseGame();

        if (winScoreText != null)
        {
            if (score >= 0f)
            {
                float rounded = Mathf.Round(score * 10f) / 10f;

                winScoreText.text =
                    $"Tu nota: {rounded:F1} / 10  —  " +
                    $"{(score >= LevelGrade.PassingScore ? "Aprobado ✓" : "Desaprobado ✗")}";

                winScoreText.gameObject.SetActive(true);
            }
            else
            {
                winScoreText.gameObject.SetActive(false);
            }
        }
    }

    public void ShowLose()
    {
        HideAll();
        SetActive(loseCanvas, true);
        PauseGame();
    }

    // ─────────────────────────────────────────────
    // Info / Hint Panel
    // ─────────────────────────────────────────────

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

    public void ShowHintPanel(ClickPoint point)
    {
        if (infoPanel == null) return;

        currentPoint = null;

        if (infoPanelTitle != null)
            infoPanelTitle.text = "Pista";

        if (infoPanelDescription != null)
            infoPanelDescription.text = point.HintText;

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

    // ─────────────────────────────────────────────
    // HUD
    // ─────────────────────────────────────────────

    public void UpdateProgressDisplay(int found, int total)
    {
        if (progressText != null)
            progressText.text = $"{found} / {total} errores encontrados";
    }

    /// <summary>
    /// Actualiza texto y reloj radial.
    /// currentTime = tiempo restante.
    /// maxTime = tiempo total del nivel.
    /// </summary>
  public void UpdateTimerDisplay(float currentTime, float maxTime)
{

    if (timerFillImage != null)
    {
            timerFillImage.fillAmount = 1f - Mathf.Clamp01(currentTime / maxTime);
    }
}

    /// <summary>
    /// Actualiza contador de clicks incorrectos.
    /// </summary>
    public void UpdateClickDisplay(int wrongClicks, int maxClicks)
    {
        if (clickCounterText == null)
            return;

        if (maxClicks <= 0)
        {
            clickCounterText.gameObject.SetActive(false);
            return;
        }

        clickCounterText.gameObject.SetActive(true);
        clickCounterText.text = $"Clicks: {wrongClicks} / {maxClicks}";
    }

    // ─────────────────────────────────────────────
    // Pistas
    // ─────────────────────────────────────────────

    public void UpdateHintButton(int hintsUsed, int maxHints)
    {
        if (hintButton == null)
            return;

        bool exhausted = hintsUsed >= maxHints;

        hintButton.interactable = !exhausted;

        Image img = hintButton.GetComponent<Image>();

        if (img != null)
        {
            img.color = exhausted
                ? hintExhaustedColor
                : hintAvailableColor;
        }

        TMP_Text label =
            hintButton.GetComponentInChildren<TMP_Text>();

        if (label != null)
        {
            label.text = exhausted
                ? "Sin pistas"
                : $"Pista ({maxHints - hintsUsed} restantes)";
        }
    }

    /// <summary>
    /// Actualiza el texto e iconos visuales de pistas.
    /// </summary>
    public void UpdateHintsDisplay(int hintsUsed, int maxHints)
    {
        int remaining = Mathf.Max(0, maxHints - hintsUsed);

        if (hintsRemainingText != null)
        {
            hintsRemainingText.text =
                $"{remaining}/{maxHints}";
        }

        if (hintIcons == null)
            return;

        for (int i = 0; i < hintIcons.Length; i++)
        {
            if (hintIcons[i] == null)
                continue;

            hintIcons[i].color =
                i < remaining
                ? hintIconAvailableColor
                : hintIconSpentColor;
        }
    }

    // ─────────────────────────────────────────────
    // Botones
    // ─────────────────────────────────────────────

    public void StartGameButton()
    {
        GetSceneController()?.StartGame();
    }

    public void HintButton()
    {
        GameManager.Instance?.ShowHint();
    }

    public void ReloadSceneButton()
    {
        GetSceneController()?.ReloadScene();
    }

    public void LoadMenuButton()
    {
        GetSceneController()?.LoadMenu();
    }

    public void LoadLevelTwo()
    {
        GetSceneController()?.LoadSecondLevel();
    }

    public void LoadLevelSelector()
    {
        SceneManager.LoadScene("LevelSelector");
    }

    // ─────────────────────────────────────────────
    // Pause / Resume
    // ─────────────────────────────────────────────

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

    // ─────────────────────────────────────────────
    // Util
    // ─────────────────────────────────────────────

    private static void SetActive(GameObject go, bool value)
    {
        if (go != null)
            go.SetActive(value);
    }
}