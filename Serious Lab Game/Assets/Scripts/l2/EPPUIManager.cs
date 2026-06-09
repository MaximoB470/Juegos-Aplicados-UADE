using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manager de UI del Nivel 2 (EPP).
/// Antes de mostrar el panel de resultado, revela una por una
/// las categorías con tilde o cruz animada para generar suspenso.
/// </summary>
public class EPPUIManager : MonoBehaviour
{
    private const string SERVICE_KEY = "EPPUIManager";
    private const string GAME_MANAGER_KEY = "EPPGameManager";
    private const string SELECTOR_SCENE = "LevelSelector";

    private EPPGameManager gameManager;
    private Coroutine currentRevealCoroutine;

    [Header("Panel de escenario")]
    [SerializeField] private TextMeshProUGUI scenarioTitleText;
    [SerializeField] private TextMeshProUGUI scenarioContextText;

    [Header("Sliders de EPP")]
    [SerializeField] private Slider headSlider;
    [SerializeField] private Slider bodySlider;
    [SerializeField] private Slider handsSlider;
    [SerializeField] private Slider feetSlider;

    [Header("Labels de opción actual")]
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

    [Header("Overlays de resultado")]
    [Tooltip("Image vacía encima de la imagen de cabeza. Empieza desactivada.")]
    [SerializeField] private Image headResultOverlay;
    [SerializeField] private Image bodyResultOverlay;
    [SerializeField] private Image handsResultOverlay;
    [SerializeField] private Image feetResultOverlay;

    [Header("Sprites de resultado")]
    [Tooltip("Sprite de tilde para respuesta correcta.")]
    [SerializeField] private Sprite correctSprite;
    [Tooltip("Sprite de X para respuesta incorrecta.")]
    [SerializeField] private Sprite incorrectSprite;

    [Header("Colores de overlay")]
    [SerializeField] private Color correctColor = new Color(0.15f, 0.80f, 0.25f, 1f);
    [SerializeField] private Color incorrectColor = new Color(0.90f, 0.20f, 0.20f, 1f);

    [Header("Timing de suspenso (segundos)")]
    [Tooltip("Pausa antes de revelar cada categoría.")]
    [SerializeField] private float suspensePause = 1.0f;
    [Tooltip("Duración de la animación de pop del overlay.")]
    [SerializeField] private float revealDuration = 0.35f;
    [Tooltip("Pausa después de revelar cada categoría.")]
    [SerializeField] private float pauseAfterReveal = 0.4f;
    [Tooltip("Pausa final antes de mostrar el panel de resultado.")]
    [SerializeField] private float finalPause = 0.7f;

    [Header("Panel de resultado")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultTitleText;
    [SerializeField] private TextMeshProUGUI resultBodyText;
    [SerializeField] private Button continueButton;

    [Header("Pantalla de fin de nivel")]
    [SerializeField] private GameObject endLevelPanel;
    [SerializeField] private TextMeshProUGUI endLevelScoreText;
    [SerializeField] private TextMeshProUGUI endLevelGradeText;
    [SerializeField] private Button endLevelContinueButton;

    private List<EPPOptionSO> currentHeadOptions;
    private List<EPPOptionSO> currentBodyOptions;
    private List<EPPOptionSO> currentHandsOptions;
    private List<EPPOptionSO> currentFeetOptions;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

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

        gameManager.OnScenarioLoaded += HandleScenarioLoaded;
        gameManager.OnResultReady += HandleResultReady;
        gameManager.OnLevelComplete += HandleLevelComplete;

        confirmButton?.onClick.AddListener(OnConfirmClicked);
        continueButton?.onClick.AddListener(OnContinueClicked);
        endLevelContinueButton?.onClick.AddListener(() => SceneManager.LoadScene(SELECTOR_SCENE));

        headSlider?.onValueChanged.AddListener(_ => RefreshSliderLabel(headSlider, currentHeadOptions, headOptionLabel, headOptionImage));
        bodySlider?.onValueChanged.AddListener(_ => RefreshSliderLabel(bodySlider, currentBodyOptions, bodyOptionLabel, bodyOptionImage));
        handsSlider?.onValueChanged.AddListener(_ => RefreshSliderLabel(handsSlider, currentHandsOptions, handsOptionLabel, handsOptionImage));
        feetSlider?.onValueChanged.AddListener(_ => RefreshSliderLabel(feetSlider, currentFeetOptions, feetOptionLabel, feetOptionImage));

        HideAllOverlays();
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

    // ─── Handlers de eventos ──────────────────────────────────────────────────

    private void HandleScenarioLoaded(EPPScenarioSO scenario)
    {
        if (currentRevealCoroutine != null)
        {
            StopCoroutine(currentRevealCoroutine);
            currentRevealCoroutine = null;
        }

        HideAllOverlays();

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

        if (confirmButton != null) confirmButton.interactable = true;

        SetResultPanelActive(false);
        SetEndLevelPanelActive(false);
    }

    /// <summary>
    /// En vez de mostrar el panel inmediatamente, arranca la secuencia animada.
    /// </summary>
    private void HandleResultReady(EPPResult result)
    {
        if (confirmButton != null) confirmButton.interactable = false;

        if (currentRevealCoroutine != null) StopCoroutine(currentRevealCoroutine);
        currentRevealCoroutine = StartCoroutine(RevealCategoriesSequence(result));
    }

    private void HandleLevelComplete(int correct, int total, float score)
    {
        SetResultPanelActive(false);
        SetEndLevelPanelActive(true);

        if (endLevelScoreText != null)
            endLevelScoreText.text = $"Respondiste correctamente {correct} de {total} categorías";

        if (endLevelGradeText != null)
        {
            float rounded = Mathf.Round(score * 10f) / 10f;
            endLevelGradeText.text = $"Tu nota: {rounded:F1} / 10  —  " +
                                     $"{(score >= LevelGrade.PassingScore ? "Aprobado ✓" : "Desaprobado ✗")}";
        }
    }

    // ─── Animación de reveal ──────────────────────────────────────────────────

    /// <summary>
    /// Recorre las 4 categorías una por una con pausa de suspenso.
    /// Muestra tilde o cruz con animación de pop sobre cada imagen.
    /// Al terminar la última, muestra el panel de resultado.
    /// </summary>
    private IEnumerator RevealCategoriesSequence(EPPResult result)
    {
        var categories = new (Image overlay, bool correct)[]
        {
            (headResultOverlay,  result.headCorrect),
            (bodyResultOverlay,  result.bodyCorrect),
            (handsResultOverlay, result.handsCorrect),
            (feetResultOverlay,  result.feetCorrect),
        };

        foreach (var (overlay, correct) in categories)
        {
            if (overlay == null) continue;

            yield return new WaitForSeconds(suspensePause);
            yield return PopOverlay(overlay, correct);
            yield return new WaitForSeconds(pauseAfterReveal);
        }

        yield return new WaitForSeconds(finalPause);

        BuildAndShowResultPanel(result);
        currentRevealCoroutine = null;
    }

    /// <summary>
    /// Activa el overlay con sprite y color correctos.
    /// Anima escala 0 → overshoot → 1 para dar sensación de impacto.
    /// </summary>
    private IEnumerator PopOverlay(Image overlay, bool correct)
    {
        overlay.sprite = correct ? correctSprite : incorrectSprite;
        overlay.color = correct ? correctColor : incorrectColor;
        overlay.gameObject.SetActive(true);
        overlay.transform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < revealDuration)
        {
            float t = Mathf.Clamp01(elapsed / revealDuration);
            overlay.transform.localScale = Vector3.one * EaseOutBack(t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        overlay.transform.localScale = Vector3.one;
    }

    /// <summary>Easing con overshoot — da sensación de "stamp" al aparecer.</summary>
    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    // ─── Handlers de botones ──────────────────────────────────────────────────

    private void OnConfirmClicked()
    {
        if (gameManager == null) return;

        int headIndex = Mathf.RoundToInt(headSlider != null ? headSlider.value : 0);
        int bodyIndex = Mathf.RoundToInt(bodySlider != null ? bodySlider.value : 0);
        int handsIndex = Mathf.RoundToInt(handsSlider != null ? handsSlider.value : 0);
        int feetIndex = Mathf.RoundToInt(feetSlider != null ? feetSlider.value : 0);

        gameManager.SubmitAnswer(headIndex, bodyIndex, handsIndex, feetIndex);
    }

    private void OnContinueClicked() => gameManager?.AdvanceToNextScenario();

    // ─── UI helpers ───────────────────────────────────────────────────────────

    private void HideAllOverlays()
    {
        HideOverlay(headResultOverlay);
        HideOverlay(bodyResultOverlay);
        HideOverlay(handsResultOverlay);
        HideOverlay(feetResultOverlay);
    }

    private static void HideOverlay(Image overlay)
    {
        if (overlay == null) return;
        overlay.gameObject.SetActive(false);
        overlay.transform.localScale = Vector3.zero;
    }

    private void BuildAndShowResultPanel(EPPResult result)
    {
        if (resultTitleText != null)
        {
            resultTitleText.text = result.allCorrect ? "¡Muy bien!" : "Revisá tu elección";
            resultTitleText.color = result.allCorrect ? Color.green : Color.red;
        }

        if (resultBodyText != null)
        {
            if (result.allCorrect)
            {
                resultBodyText.text = result.scenarioFeedback;
            }
            else
            {
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < result.incorrectCategoryNames.Count; i++)
                    sb.AppendLine($"• {result.incorrectCategoryNames[i]}: " +
                                  $"elegiste \"{result.incorrectLabels[i]}\" " +
                                  $"— lo correcto era \"{result.correctLabels[i]}\"");
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
        EPPOptionSO opt = options[index];
        label.text = opt != null ? opt.optionLabel : "—";
        if (image != null && opt != null)
        {
            image.sprite = opt.optionIcon;
            image.gameObject.SetActive(opt.optionIcon != null);
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