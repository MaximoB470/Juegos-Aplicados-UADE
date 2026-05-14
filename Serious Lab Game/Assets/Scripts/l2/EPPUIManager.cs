using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manager de UI del nivel EPP.
/// - Se registra en el ServiceLocator con la clave "EPPUIManager".
/// - Solo presenta datos y delega acciones al EPPGameManager.
/// - No contiene lógica de evaluación ni de estado de juego.
/// - Al completar el nivel notifica al LevelProgressService y vuelve al selector.
/// </summary>
public class EPPUIManager : MonoBehaviour
{
    private const string SERVICE_KEY = "EPPUIManager";
    private const string GAME_MANAGER_KEY = "EPPGameManager";
    private const string PROGRESS_KEY = "LevelProgressService";
    private const string SELECTOR_SCENE = "LevelSelector";

    private EPPGameManager gameManager;
    private LevelProgressService progressService;

    [Header("Panel de escenario")]
    [SerializeField] private TextMeshProUGUI scenarioTitleText;
    [SerializeField] private TextMeshProUGUI scenarioContextText;

    [Header("Sliders de EPP")]
    [SerializeField] private Slider headSlider;
    [SerializeField] private Slider bodySlider;
    [SerializeField] private Slider handsSlider;
    [SerializeField] private Slider feetSlider;

    [Header("Labels de opción actual (actualizados en tiempo real)")]
    [SerializeField] private TextMeshProUGUI headOptionLabel;
    [SerializeField] private TextMeshProUGUI bodyOptionLabel;
    [SerializeField] private TextMeshProUGUI handsOptionLabel;
    [SerializeField] private TextMeshProUGUI feetOptionLabel;

    [Header("Imágenes de opción actual")]
    [SerializeField] private Image headOptionImage;
    [SerializeField] private Image bodyOptionImage;
    [SerializeField] private Image handsOptionImage;
    [SerializeField] private Image feetOptionImage;

    [Header("Botón confirmar")]
    [SerializeField] private Button confirmButton;

    [Header("Panel de resultado")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultTitleText;
    [SerializeField] private TextMeshProUGUI resultBodyText;
    [SerializeField] private Button continueButton;

    [Header("Pantalla de fin de nivel")]
    [SerializeField] private GameObject endLevelPanel;
    [SerializeField] private TextMeshProUGUI endLevelScoreText;
    [SerializeField] private Button endLevelContinueButton;

    private List<EPPOptionSO> currentHeadOptions;
    private List<EPPOptionSO> currentBodyOptions;
    private List<EPPOptionSO> currentHandsOptions;
    private List<EPPOptionSO> currentFeetOptions;

    // ─── Lifecycle ───────────────────────────────────────────────────────────

    private void Awake()
    {
        ServiceLocator.Instance.SetService(SERVICE_KEY, this);
    }

    private void Start()
    {
        gameManager = ServiceLocator.Instance.GetService(GAME_MANAGER_KEY) as EPPGameManager;

        if (gameManager == null)
        {
            Debug.LogError("[EPPUIManager] No se encontró EPPGameManager en el ServiceLocator.");
            return;
        }

        progressService = ServiceLocator.Instance.GetService(PROGRESS_KEY) as LevelProgressService;

        if (progressService == null)
            Debug.LogWarning("[EPPUIManager] No se encontró LevelProgressService. El progreso no se guardará.");

        // Eventos del GameManager
        gameManager.OnScenarioLoaded += HandleScenarioLoaded;
        gameManager.OnResultReady += HandleResultReady;
        gameManager.OnLevelComplete += HandleLevelComplete;

        // Botones
        confirmButton?.onClick.AddListener(OnConfirmClicked);
        continueButton?.onClick.AddListener(OnContinueClicked);
        endLevelContinueButton?.onClick.AddListener(OnEndLevelContinueClicked);

        // Sliders
        headSlider?.onValueChanged.AddListener(_ =>
            RefreshSliderLabel(headSlider, currentHeadOptions, headOptionLabel, headOptionImage));
        bodySlider?.onValueChanged.AddListener(_ =>
            RefreshSliderLabel(bodySlider, currentBodyOptions, bodyOptionLabel, bodyOptionImage));
        handsSlider?.onValueChanged.AddListener(_ =>
            RefreshSliderLabel(handsSlider, currentHandsOptions, handsOptionLabel, handsOptionImage));
        feetSlider?.onValueChanged.AddListener(_ =>
            RefreshSliderLabel(feetSlider, currentFeetOptions, feetOptionLabel, feetOptionImage));

        SetResultPanelActive(false);
        SetEndLevelPanelActive(false);
    }

    private void OnDestroy()
    {
        if (gameManager == null) return;

        gameManager.OnScenarioLoaded -= HandleScenarioLoaded;
        gameManager.OnResultReady -= HandleResultReady;
        gameManager.OnLevelComplete -= HandleLevelComplete;
    }

    // ─── Handlers de eventos ─────────────────────────────────────────────────

    private void HandleScenarioLoaded(EPPScenarioSO scenario)
    {
        if (scenarioTitleText != null) scenarioTitleText.text = scenario.scenarioTitle;
        if (scenarioContextText != null) scenarioContextText.text = scenario.scenarioContext;

        currentHeadOptions = scenario.headOptions;
        currentBodyOptions = scenario.bodyOptions;
        currentHandsOptions = scenario.handsOptions;
        currentFeetOptions = scenario.feetOptions;

        ConfigureSlider(headSlider, currentHeadOptions);
        ConfigureSlider(bodySlider, currentBodyOptions);
        ConfigureSlider(handsSlider, currentHandsOptions);
        ConfigureSlider(feetSlider, currentFeetOptions);

        ResetSlider(headSlider, currentHeadOptions, headOptionLabel, headOptionImage);
        ResetSlider(bodySlider, currentBodyOptions, bodyOptionLabel, bodyOptionImage);
        ResetSlider(handsSlider, currentHandsOptions, handsOptionLabel, handsOptionImage);
        ResetSlider(feetSlider, currentFeetOptions, feetOptionLabel, feetOptionImage);

        SetResultPanelActive(false);
        SetEndLevelPanelActive(false);
    }

    private void HandleResultReady(EPPResult result)
    {
        BuildAndShowResultPanel(result);
    }

    private void HandleLevelComplete(int correct, int total)
    {
        progressService?.CompleteCurrentLevel();

        SetResultPanelActive(false);
        SetEndLevelPanelActive(true);

        if (endLevelScoreText != null)
            endLevelScoreText.text = $"Acertaste {correct} de {total} situaciones";
    }

    // ─── Handlers de botones ─────────────────────────────────────────────────

    private void OnConfirmClicked()
    {
        if (gameManager == null) return;

        int headIndex = Mathf.RoundToInt(headSlider != null ? headSlider.value : 0);
        int bodyIndex = Mathf.RoundToInt(bodySlider != null ? bodySlider.value : 0);
        int handsIndex = Mathf.RoundToInt(handsSlider != null ? handsSlider.value : 0);
        int feetIndex = Mathf.RoundToInt(feetSlider != null ? feetSlider.value : 0);

        gameManager.SubmitAnswer(headIndex, bodyIndex, handsIndex, feetIndex);
    }

    private void OnContinueClicked()
    {
        gameManager?.AdvanceToNextScenario();
    }

    private void OnEndLevelContinueClicked()
    {
        SceneManager.LoadScene(SELECTOR_SCENE);
    }

    // ─── UI helpers ──────────────────────────────────────────────────────────

    private void BuildAndShowResultPanel(EPPResult result)
    {
        if (result.allCorrect)
        {
            if (resultTitleText != null)
            {
                resultTitleText.text = "¡Muy bien!";
                resultTitleText.color = Color.green;
            }

            if (resultBodyText != null)
                resultBodyText.text = result.scenarioFeedback;
        }
        else
        {
            if (resultTitleText != null)
            {
                resultTitleText.text = "Revisá tu elección";
                resultTitleText.color = Color.red;
            }

            if (resultBodyText != null)
            {
                var sb = new System.Text.StringBuilder();

                for (int i = 0; i < result.incorrectCategoryNames.Count; i++)
                {
                    string category = result.incorrectCategoryNames[i];
                    string chosen = result.incorrectLabels[i];
                    string correct = result.correctLabels[i];

                    sb.AppendLine($"• {category}: elegiste \"{chosen}\" — lo correcto era \"{correct}\"");
                }

                sb.AppendLine();
                sb.Append(result.scenarioFeedback);

                resultBodyText.text = sb.ToString();
            }
        }

        SetResultPanelActive(true);
    }

    private void ConfigureSlider(Slider slider, List<EPPOptionSO> options)
    {
        if (slider == null || options == null) return;

        slider.minValue = 0;
        slider.maxValue = Mathf.Max(0, options.Count - 1);
        slider.wholeNumbers = true;
    }

    private void ResetSlider(Slider slider, List<EPPOptionSO> options,
                              TextMeshProUGUI label, Image image)
    {
        if (slider == null) return;

        slider.value = 0;
        RefreshSliderLabel(slider, options, label, image);
    }

    private void RefreshSliderLabel(Slider slider, List<EPPOptionSO> options,
                                    TextMeshProUGUI label, Image image)
    {
        if (slider == null || options == null || label == null) return;

        int index = Mathf.Clamp(Mathf.RoundToInt(slider.value), 0, options.Count - 1);

        EPPOptionSO option = options[index];
        label.text = option != null ? option.optionLabel : "—";

        if (image != null && option != null)
        {
            image.sprite = option.optionIcon;
            image.gameObject.SetActive(option.optionIcon != null);
        }
    }

    private void SetResultPanelActive(bool active)
    {
        if (resultPanel != null) resultPanel.SetActive(active);
    }

    private void SetEndLevelPanelActive(bool active)
    {
        if (endLevelPanel != null) endLevelPanel.SetActive(active);
    }
}