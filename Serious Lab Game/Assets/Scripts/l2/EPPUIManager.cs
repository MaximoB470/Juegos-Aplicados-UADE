using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manager de UI del nivel EPP.
/// - Se registra en el ServiceLocator con la clave "EPPUIManager".
/// - Solo presenta datos y delega acciones al EPPGameManager.
/// - No contiene lógica de evaluación ni de estado de juego.
/// </summary>
public class EPPUIManager : MonoBehaviour
{
    // ── Registro en ServiceLocator ────────────────────────────────────────────
    private const string SERVICE_KEY      = "EPPUIManager";
    private const string GAME_MANAGER_KEY = "EPPGameManager";

    // ── Referencia al manager de lógica ──────────────────────────────────────
    private EPPGameManager gameManager;

    // ── UI: panel principal del escenario ─────────────────────────────────────
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

    [Header("Botón confirmar")]
    [SerializeField] private Button confirmButton;

    // ── UI: panel de resultado ────────────────────────────────────────────────
    [Header("Panel de resultado")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultTitleText;
    [SerializeField] private TextMeshProUGUI resultBodyText;
    [SerializeField] private Button continueButton;

    // ── UI: pantalla de fin de nivel ──────────────────────────────────────────
    [Header("Pantalla de fin de nivel")]
    [SerializeField] private GameObject endLevelPanel;
    [SerializeField] private TextMeshProUGUI endLevelScoreText;

    // ── Estado interno de UI ──────────────────────────────────────────────────
    // Guardamos las listas de opciones del escenario actual para construir labels
    private List<EPPOptionSO> currentHeadOptions;
    private List<EPPOptionSO> currentBodyOptions;
    private List<EPPOptionSO> currentHandsOptions;
    private List<EPPOptionSO> currentFeetOptions;

    // ── Ciclo de vida ─────────────────────────────────────────────────────────

    private void Awake()
    {
        ServiceLocator.Instance.SetService(SERVICE_KEY, this);
    }

    private void Start()
    {
        // Obtener EPPGameManager desde el ServiceLocator
        gameManager = ServiceLocator.Instance.GetService(GAME_MANAGER_KEY) as EPPGameManager;

        if (gameManager == null)
        {
            Debug.LogError("[EPPUIManager] No se encontró EPPGameManager en el ServiceLocator.");
            return;
        }

        // Suscribirse a los eventos
        gameManager.OnScenarioLoaded  += HandleScenarioLoaded;
        gameManager.OnResultReady     += HandleResultReady;
        gameManager.OnLevelComplete   += HandleLevelComplete;

        // Vincular botones
        confirmButton?.onClick.AddListener(OnConfirmClicked);
        continueButton?.onClick.AddListener(OnContinueClicked);

        // Vincular sliders para actualizar labels en tiempo real
        headSlider?.onValueChanged.AddListener(_ => RefreshSliderLabel(headSlider,  currentHeadOptions,  headOptionLabel));
        bodySlider?.onValueChanged.AddListener(_ => RefreshSliderLabel(bodySlider,  currentBodyOptions,  bodyOptionLabel));
        handsSlider?.onValueChanged.AddListener(_ => RefreshSliderLabel(handsSlider, currentHandsOptions, handsOptionLabel));
        feetSlider?.onValueChanged.AddListener(_ => RefreshSliderLabel(feetSlider,  currentFeetOptions,  feetOptionLabel));

        // Estado inicial
        SetResultPanelActive(false);
        SetEndLevelPanelActive(false);
    }

    private void OnDestroy()
    {
        // Desuscribirse para evitar memory leaks
        if (gameManager == null) return;

        gameManager.OnScenarioLoaded  -= HandleScenarioLoaded;
        gameManager.OnResultReady     -= HandleResultReady;
        gameManager.OnLevelComplete   -= HandleLevelComplete;
    }

    // ── Handlers de eventos ───────────────────────────────────────────────────

    private void HandleScenarioLoaded(EPPScenarioSO scenario)
    {
        // Texto del escenario
        if (scenarioTitleText   != null) scenarioTitleText.text   = scenario.scenarioTitle;
        if (scenarioContextText != null) scenarioContextText.text = scenario.scenarioContext;

        // Guardar listas de opciones en estado local
        currentHeadOptions  = scenario.headOptions;
        currentBodyOptions  = scenario.bodyOptions;
        currentHandsOptions = scenario.handsOptions;
        currentFeetOptions  = scenario.feetOptions;

        // Configurar sliders (min siempre 0, whole numbers)
        ConfigureSlider(headSlider,  currentHeadOptions);
        ConfigureSlider(bodySlider,  currentBodyOptions);
        ConfigureSlider(handsSlider, currentHandsOptions);
        ConfigureSlider(feetSlider,  currentFeetOptions);

        // Resetear a la primera opción y actualizar labels
        ResetSlider(headSlider,  currentHeadOptions,  headOptionLabel);
        ResetSlider(bodySlider,  currentBodyOptions,  bodyOptionLabel);
        ResetSlider(handsSlider, currentHandsOptions, handsOptionLabel);
        ResetSlider(feetSlider,  currentFeetOptions,  feetOptionLabel);

        // Ocultar panel de resultado al cargar nuevo escenario
        SetResultPanelActive(false);
        SetEndLevelPanelActive(false);
    }

    private void HandleResultReady(EPPResult result)
    {
        BuildAndShowResultPanel(result);
    }

    private void HandleLevelComplete(int correct, int total)
    {
        SetResultPanelActive(false);
        SetEndLevelPanelActive(true);

        if (endLevelScoreText != null)
            endLevelScoreText.text = $"Acertaste {correct} de {total} situaciones";
    }

    // ── Handlers de botones ───────────────────────────────────────────────────

    private void OnConfirmClicked()
    {
        if (gameManager == null) return;

        int headIndex  = Mathf.RoundToInt(headSlider  != null ? headSlider.value  : 0);
        int bodyIndex  = Mathf.RoundToInt(bodySlider  != null ? bodySlider.value  : 0);
        int handsIndex = Mathf.RoundToInt(handsSlider != null ? handsSlider.value : 0);
        int feetIndex  = Mathf.RoundToInt(feetSlider  != null ? feetSlider.value  : 0);

        gameManager.SubmitAnswer(headIndex, bodyIndex, handsIndex, feetIndex);
    }

    private void OnContinueClicked()
    {
        gameManager?.AdvanceToNextScenario();
    }

    // ── Construcción del panel de resultado ───────────────────────────────────

    private void BuildAndShowResultPanel(EPPResult result)
    {
        if (result.allCorrect)
        {
            // ── Respuesta perfecta ──────────────────────────────────────────
            if (resultTitleText != null)
            {
                resultTitleText.text  = "¡Muy bien!";
                resultTitleText.color = Color.green;
            }

            if (resultBodyText != null)
                resultBodyText.text = result.scenarioFeedback;
        }
        else
        {
            // ── Hay errores ─────────────────────────────────────────────────
            if (resultTitleText != null)
            {
                resultTitleText.text  = "Revisá tu elección";
                resultTitleText.color = Color.red;
            }

            if (resultBodyText != null)
            {
                var sb = new System.Text.StringBuilder();

                // Desglose por categoría fallada
                for (int i = 0; i < result.incorrectCategoryNames.Count; i++)
                {
                    string category   = result.incorrectCategoryNames[i];
                    string chosen     = result.incorrectLabels[i];
                    string correct    = result.correctLabels[i];

                    sb.AppendLine($"• {category}: elegiste \"{chosen}\" — lo correcto era \"{correct}\"");
                }

                // Feedback del escenario al final
                sb.AppendLine();
                sb.Append(result.scenarioFeedback);

                resultBodyText.text = sb.ToString();
            }
        }

        SetResultPanelActive(true);
    }

    // ── Helpers de sliders ────────────────────────────────────────────────────

    /// <summary>
    /// Configura el rango del slider según la cantidad de opciones disponibles.
    /// </summary>
    private void ConfigureSlider(Slider slider, List<EPPOptionSO> options)
    {
        if (slider == null || options == null) return;

        slider.minValue    = 0;
        slider.maxValue    = Mathf.Max(0, options.Count - 1);
        slider.wholeNumbers = true;
    }

    /// <summary>
    /// Resetea el slider a 0 y actualiza el label de la primera opción.
    /// </summary>
    private void ResetSlider(Slider slider, List<EPPOptionSO> options, TextMeshProUGUI label)
    {
        if (slider == null) return;

        slider.value = 0;
        RefreshSliderLabel(slider, options, label);
    }

    /// <summary>
    /// Actualiza el label de texto según el valor actual del slider.
    /// </summary>
    private void RefreshSliderLabel(Slider slider, List<EPPOptionSO> options, TextMeshProUGUI label)
    {
        if (slider == null || options == null || label == null) return;

        int index = Mathf.RoundToInt(slider.value);
        index = Mathf.Clamp(index, 0, options.Count - 1);

        label.text = options[index] != null ? options[index].optionLabel : "—";
    }

    // ── Helpers de visibilidad ────────────────────────────────────────────────

    private void SetResultPanelActive(bool active)
    {
        if (resultPanel != null) resultPanel.SetActive(active);
    }

    private void SetEndLevelPanelActive(bool active)
    {
        if (endLevelPanel != null) endLevelPanel.SetActive(active);
    }
}
