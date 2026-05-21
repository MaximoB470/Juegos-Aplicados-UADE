using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manager de UI del nivel Quiz (Nivel 3).
/// - Se registra en el ServiceLocator con la clave "QuizUIManager".
/// - Solo presenta datos y delega acciones al QuizGameManager.
/// - No contiene lógica de evaluación ni de estado de juego.
/// </summary>
public class QuizUIManager : MonoBehaviour
{
    private const string SERVICE_KEY      = "QuizUIManager";
    private const string GAME_MANAGER_KEY = "QuizGameManager";
    private const string PROGRESS_KEY     = "LevelProgressService";
    private const string SELECTOR_SCENE   = "LevelSelector";

    private QuizGameManager      gameManager;
    private LevelProgressService progressService;

    [Header("Panel de pregunta")]
    [SerializeField] private TMP_Text situationText;
    [SerializeField] private Slider   timerSlider;
    [SerializeField] private TMP_Text timerLabel;

    [Header("Botones de opciones — asigná exactamente 3")]
    [SerializeField] private List<Button>    optionButtons;
    [SerializeField] private List<TMP_Text>  optionLabels;

    [Header("Panel de resultado")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text   resultTitleText;
    [SerializeField] private TMP_Text   resultBodyText;
    [SerializeField] private Button     continueButton;

    [Header("Panel de timeout")]
    [SerializeField] private GameObject timeoutPanel;
    [SerializeField] private Button     timeoutContinueButton;

    [Header("Pantalla de fin de quiz")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text   endScoreText;
    [SerializeField] private Button     endContinueButton;

    // Cache de la situación actual para mapear botón → opción
    private List<QuizOptionSO> currentOptions = new();

    // ─── Lifecycle ───────────────────────────────────────────────────────────

    private void Awake()
    {
        ServiceLocator.Instance.SetService(SERVICE_KEY, this);
    }

    private void Start()
    {
        gameManager = ServiceLocator.Instance.GetService(GAME_MANAGER_KEY) as QuizGameManager;

        if (gameManager == null)
        {
            Debug.LogError("[QuizUIManager] No se encontró QuizGameManager en el ServiceLocator.");
            return;
        }

        progressService = ServiceLocator.Instance.GetService(PROGRESS_KEY) as LevelProgressService;

        // Suscribir eventos
        gameManager.OnSituationLoaded += HandleSituationLoaded;
        gameManager.OnAnswerResult    += HandleAnswerResult;
        gameManager.OnTimerUpdated    += HandleTimerUpdated;
        gameManager.OnTimeOut         += HandleTimeOut;
        gameManager.OnQuizComplete    += HandleQuizComplete;

        // Suscribir botones de opciones
        for (int i = 0; i < optionButtons.Count; i++)
        {
            int index = i; // closure
            optionButtons[i]?.onClick.AddListener(() => OnOptionClicked(index));
        }

        continueButton?.onClick.AddListener(OnContinueClicked);
        timeoutContinueButton?.onClick.AddListener(OnTimeoutContinueClicked);
        endContinueButton?.onClick.AddListener(OnEndContinueClicked);

        SetResultPanelActive(false);
        SetTimeoutPanelActive(false);
        SetEndPanelActive(false);
    }

    private void OnDestroy()
    {
        if (gameManager == null) return;

        gameManager.OnSituationLoaded -= HandleSituationLoaded;
        gameManager.OnAnswerResult    -= HandleAnswerResult;
        gameManager.OnTimerUpdated    -= HandleTimerUpdated;
        gameManager.OnTimeOut         -= HandleTimeOut;
        gameManager.OnQuizComplete    -= HandleQuizComplete;
    }

    // ─── Handlers de eventos ─────────────────────────────────────────────────

    private void HandleSituationLoaded(QuizSituationSO situation)
    {
        // Sobreescribir texto de situación
        if (situationText != null)
            situationText.text = situation.situationText;

        // Cachear opciones y sobreescribir labels de botones
        currentOptions = situation.options;

        for (int i = 0; i < optionButtons.Count; i++)
        {
            bool hasOption = i < currentOptions.Count;

            if (optionButtons[i] != null)
            {
                optionButtons[i].gameObject.SetActive(hasOption);
                optionButtons[i].interactable = true;
            }

            if (hasOption && optionLabels[i] != null)
                optionLabels[i].text = currentOptions[i].optionText;
        }

        SetResultPanelActive(false);
        SetTimeoutPanelActive(false);
    }

    private void HandleAnswerResult(QuizOptionSO option, bool correct)
    {
        // Deshabilitar botones para que no puedan seguir clickeando
        SetOptionButtonsInteractable(false);

        if (resultTitleText != null)
        {
            resultTitleText.text  = correct ? "¡Correcto!" : "Incorrecto";
            resultTitleText.color = correct ? Color.green  : Color.red;
        }

        if (resultBodyText != null)
            resultBodyText.text = correct ? option.correctFeedback : option.incorrectFeedback;

        SetResultPanelActive(true);
    }

    private void HandleTimerUpdated(float remaining, float total)
    {
        if (timerSlider != null)
        {
            timerSlider.minValue = 0f;
            timerSlider.maxValue = total;
            timerSlider.value    = remaining;
        }

        if (timerLabel != null)
            timerLabel.text = Mathf.CeilToInt(remaining).ToString();
    }

    private void HandleTimeOut()
    {
        SetOptionButtonsInteractable(false);
        SetTimeoutPanelActive(true);
    }

    private void HandleQuizComplete(int correct, int total)
    {
        progressService?.CompleteCurrentLevel();

        SetResultPanelActive(false);
        SetTimeoutPanelActive(false);
        SetEndPanelActive(true);

        if (endScoreText != null)
            endScoreText.text = $"Respondiste correctamente {correct} de {total} situaciones";
    }

    // ─── Handlers de botones ─────────────────────────────────────────────────

    private void OnOptionClicked(int index)
    {
        if (index >= currentOptions.Count) return;
        gameManager.SubmitAnswer(currentOptions[index]);
    }

    private void OnContinueClicked()
    {
        SetResultPanelActive(false);
        gameManager?.AdvanceToNext();
    }

    private void OnTimeoutContinueClicked()
    {
        SetTimeoutPanelActive(false);
        gameManager?.AdvanceToNext();
    }

    private void OnEndContinueClicked()
    {
        SceneManager.LoadScene(SELECTOR_SCENE);
    }

    // ─── UI helpers ──────────────────────────────────────────────────────────

    private void SetOptionButtonsInteractable(bool interactable)
    {
        foreach (var btn in optionButtons)
            if (btn != null) btn.interactable = interactable;
    }

    private void SetResultPanelActive(bool active)
    {
        if (resultPanel != null) resultPanel.SetActive(active);
    }

    private void SetTimeoutPanelActive(bool active)
    {
        if (timeoutPanel != null) timeoutPanel.SetActive(active);
    }

    private void SetEndPanelActive(bool active)
    {
        if (endPanel != null) endPanel.SetActive(active);
    }
}
