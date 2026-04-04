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
    private const string SERVICE_KEY      = "EPPUIManager";
    private const string GAME_MANAGER_KEY = "EPPGameManager";

    private EPPGameManager gameManager;

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

    private List<EPPOptionSO> currentHeadOptions;
    private List<EPPOptionSO> currentBodyOptions;
    private List<EPPOptionSO> currentHandsOptions;
    private List<EPPOptionSO> currentFeetOptions;

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

        gameManager.OnScenarioLoaded  += HandleScenarioLoaded;
        gameManager.OnResultReady     += HandleResultReady;
        gameManager.OnLevelComplete   += HandleLevelComplete;
        confirmButton?.onClick.AddListener(OnConfirmClicked);
        continueButton?.onClick.AddListener(OnContinueClicked);
        headSlider?.onValueChanged.AddListener(_ => RefreshSliderLabel(headSlider,  currentHeadOptions,  headOptionLabel, headOptionImage));
        bodySlider?.onValueChanged.AddListener(_ => RefreshSliderLabel(bodySlider,  currentBodyOptions,  bodyOptionLabel, bodyOptionImage));
        handsSlider?.onValueChanged.AddListener(_ => RefreshSliderLabel(handsSlider, currentHandsOptions, handsOptionLabel, handsOptionImage));
        feetSlider?.onValueChanged.AddListener(_ => RefreshSliderLabel(feetSlider,  currentFeetOptions,  feetOptionLabel, feetOptionImage));

        SetResultPanelActive(false);
        SetEndLevelPanelActive(false);
    }

    private void OnDestroy()
    {
        if (gameManager == null) return;

        gameManager.OnScenarioLoaded  -= HandleScenarioLoaded;
        gameManager.OnResultReady     -= HandleResultReady;
        gameManager.OnLevelComplete   -= HandleLevelComplete;
    }

    private void HandleScenarioLoaded(EPPScenarioSO scenario)
    {
        if (scenarioTitleText   != null) scenarioTitleText.text   = scenario.scenarioTitle;
        if (scenarioContextText != null) scenarioContextText.text = scenario.scenarioContext;

        currentHeadOptions  = scenario.headOptions;
        currentBodyOptions  = scenario.bodyOptions;
        currentHandsOptions = scenario.handsOptions;
        currentFeetOptions  = scenario.feetOptions;

        ConfigureSlider(headSlider,  currentHeadOptions);
        ConfigureSlider(bodySlider,  currentBodyOptions);
        ConfigureSlider(handsSlider, currentHandsOptions);
        ConfigureSlider(feetSlider,  currentFeetOptions);

        ResetSlider(headSlider,  currentHeadOptions,  headOptionLabel, headOptionImage);
        ResetSlider(bodySlider,  currentBodyOptions,  bodyOptionLabel, bodyOptionImage);
        ResetSlider(handsSlider, currentHandsOptions, handsOptionLabel, handsOptionImage);
        ResetSlider(feetSlider,  currentFeetOptions,  feetOptionLabel, feetOptionImage);

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


    private void BuildAndShowResultPanel(EPPResult result)
    {
        if (result.allCorrect)
        {
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
            if (resultTitleText != null)
            {
                resultTitleText.text  = "Revisá tu elección";
                resultTitleText.color = Color.red;
            }

            if (resultBodyText != null)
            {
                var sb = new System.Text.StringBuilder();

                for (int i = 0; i < result.incorrectCategoryNames.Count; i++)
                {
                    string category   = result.incorrectCategoryNames[i];
                    string chosen     = result.incorrectLabels[i];
                    string correct    = result.correctLabels[i];

                    sb.AppendLine($"• {category}: elegiste \"{chosen}\" — lo correcto era \"{correct}\"");
                }

                sb.AppendLine();
                sb.Append(result.scenarioFeedback);

                resultBodyText.text = sb.ToString();
            }
        }

        SetResultPanelActive(true);
    }

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
    private void ResetSlider(Slider slider, List<EPPOptionSO> options, TextMeshProUGUI label, Image image)
    {
        if (slider == null) return;

        slider.value = 0;
        RefreshSliderLabel(slider, options, label, image);
    }

    /// <summary>
    /// Actualiza el label de texto y la imagen según el valor actual del slider.
    /// </summary>
    private void RefreshSliderLabel(Slider slider, List<EPPOptionSO> options, TextMeshProUGUI label, Image image)
    {
        if (slider == null || options == null || label == null) return;

        int index = Mathf.RoundToInt(slider.value);
        index = Mathf.Clamp(index, 0, options.Count - 1);

        var option = options[index];
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
