using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class QuizUIManager : MonoBehaviour
{
    private const string SERVICE_KEY = "QuizUIManager";
    private const string GAME_MANAGER_KEY = "QuizGameManager";
    private const string SELECTOR_SCENE = "LevelSelector";

    private QuizGameManager gameManager;
    private QuizAudioManager audioManager;

    [Header("Panel de pregunta")]
    [SerializeField] private TMP_Text situationText;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private TMP_Text timerLabel;

    [Header("Botones de opciones — exactamente 3")]
    [SerializeField] private List<Button> optionButtons;
    [SerializeField] private List<TMP_Text> optionLabels;

    [Header("Panel de resultado")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text resultBodyText;
    [SerializeField] private Button continueButton;

    [Header("Panel de timeout")]
    [SerializeField] private GameObject timeoutPanel;
    [SerializeField] private TMP_Text timeoutMessageText;
    [SerializeField] private Button timeoutContinueButton;

    [Header("Pantalla de fin de quiz")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text endScoreText;
    [SerializeField] private TMP_Text endGradeText;
    [SerializeField] private Button endContinueButton;

    [Header("Tiempos de animación")]
    [SerializeField] private float situationFadeDuration = 0.6f;
    [SerializeField] private float readingPause = 2.0f;
    [SerializeField] private float optionStaggerDelay = 0.35f;
    [SerializeField] private float optionRevealDuration = 0.4f;
    [SerializeField] private float suspenseDuration = 1.8f;
    [SerializeField] private float correctRevealDuration = 0.5f;
    [SerializeField] private float timerWarningThreshold = 5f;

    // ─── Colores y estado original de botones ─────────────────────────────────

    [SerializeField] private Color buttonDefaultColor = new Color(0.15f, 0.15f, 0.25f, 1f);
    [SerializeField] private Color buttonSelectedColor = new Color(1f, 0.85f, 0.1f, 1f);
    [SerializeField] private Color buttonCorrectColor = new Color(0.1f, 0.85f, 0.3f, 1f);
    [SerializeField] private Color buttonIncorrectColor = new Color(0.85f, 0.1f, 0.1f, 1f);
    [SerializeField] private Color timerNormalColor = Color.white;
    [SerializeField] private Color timerWarningColor = new Color(1f, 0.35f, 0.1f, 1f);

    private List<Image> optionButtonImages = new();
    private List<Vector3> optionOriginalScales = new();
    private Color situationOriginalColor;
    private Color timerLabelOriginalColor;

    private List<QuizOptionSO> currentOptions = new();
    private bool timerEnabled = false;

    private Coroutine timerPulseCoroutine;
    private Coroutine revealCoroutine;

    // ─── Evento para el audio manager ─────────────────────────────────────────

    public event Action<bool> OnResultRevealed;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        ServiceLocator.Instance.SetService(SERVICE_KEY, this);

        foreach (var btn in optionButtons)
        {
            if (btn != null)
            {
                optionButtonImages.Add(btn.GetComponent<Image>());
                optionOriginalScales.Add(btn.transform.localScale);
            }
            else
            {
                optionButtonImages.Add(null);
                optionOriginalScales.Add(Vector3.one);
            }
        }

        if (situationText != null)
            situationOriginalColor = situationText.color;

        if (timerLabel != null)
            timerLabelOriginalColor = timerLabel.color;
    }

    private void Start()
    {
        gameManager = ServiceLocator.Instance.GetService(GAME_MANAGER_KEY) as QuizGameManager;
        audioManager = GetComponent<QuizAudioManager>();

        if (gameManager == null)
        {
            Debug.LogError("[QuizUIManager] No se encontró QuizGameManager en el ServiceLocator.");
            return;
        }

        gameManager.OnSituationLoaded += HandleSituationLoaded;
        gameManager.OnAnswerResult += HandleAnswerResult;
        gameManager.OnTimerUpdated += HandleTimerUpdated;
        gameManager.OnTimeOut += HandleTimeOut;
        gameManager.OnQuizComplete += HandleQuizComplete;

        for (int i = 0; i < optionButtons.Count; i++)
        {
            int index = i;
            optionButtons[i]?.onClick.AddListener(() => OnOptionClicked(index));
        }

        continueButton?.onClick.AddListener(OnContinueClicked);
        timeoutContinueButton?.onClick.AddListener(OnTimeoutContinueClicked);
        endContinueButton?.onClick.AddListener(() => SceneManager.LoadScene(SELECTOR_SCENE));

        SetResultPanelActive(false);
        SetTimeoutPanelActive(false);
        SetEndPanelActive(false);

        if (timeoutMessageText != null)
            timeoutMessageText.text =
                "Se acabó el tiempo, pero eso también es parte del aprendizaje.\n" +
                "En el laboratorio real vas a recordar que cada segundo cuenta.\n" +
                "¡Vamos por la siguiente!";
    }

    private void OnDestroy()
    {
        if (gameManager == null) return;
        gameManager.OnSituationLoaded -= HandleSituationLoaded;
        gameManager.OnAnswerResult -= HandleAnswerResult;
        gameManager.OnTimerUpdated -= HandleTimerUpdated;
        gameManager.OnTimeOut -= HandleTimeOut;
        gameManager.OnQuizComplete -= HandleQuizComplete;
    }

    // ─── Handlers de eventos ──────────────────────────────────────────────────

    private void HandleSituationLoaded(QuizSituationSO situation)
    {
        if (revealCoroutine != null) StopCoroutine(revealCoroutine);
        if (timerPulseCoroutine != null) StopCoroutine(timerPulseCoroutine);

        timerEnabled = false;

        SetResultPanelActive(false);
        SetTimeoutPanelActive(false);

        ResetAllOptionsToDefault();

        if (timerSlider != null)
            timerSlider.value = timerSlider.maxValue;
        if (timerLabel != null)
            timerLabel.color = timerLabelOriginalColor;

        currentOptions = new List<QuizOptionSO>(gameManager.GetCurrentOptions());
        revealCoroutine = StartCoroutine(RevealSituationSequence(situation));
    }

    private IEnumerator RevealSituationSequence(QuizSituationSO situation)
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (optionButtons[i] != null)
            {
                bool hasOption = i < currentOptions.Count;
                optionButtons[i].gameObject.SetActive(hasOption);
                optionButtons[i].interactable = false;
                if (hasOption)
                    optionButtons[i].transform.localScale = Vector3.zero;
            }

            if (optionLabels != null && i < optionLabels.Count && optionLabels[i] != null)
                optionLabels[i].text = string.Empty;
        }

        if (situationText != null)
        {
            situationText.text = situation.situationText;
            situationText.color = new Color(situationOriginalColor.r, situationOriginalColor.g,
                                            situationOriginalColor.b, 0f);
            yield return StartCoroutine(FadeTextAlpha(situationText, 0f, 1f, situationFadeDuration));
        }

        yield return new WaitForSeconds(readingPause);

        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (i >= currentOptions.Count) break;

            if (i < optionButtonImages.Count && optionButtonImages[i] != null)
                optionButtonImages[i].color = buttonDefaultColor;

            yield return StartCoroutine(ScalePunchReveal(
                optionButtons[i].transform,
                optionOriginalScales[i],
                optionRevealDuration));

            if (optionLabels != null && i < optionLabels.Count && optionLabels[i] != null)
                optionLabels[i].text = currentOptions[i].optionText;

            yield return new WaitForSeconds(optionStaggerDelay);
        }

        yield return new WaitForSeconds(0.15f);

        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (i < currentOptions.Count && optionButtons[i] != null)
                optionButtons[i].interactable = true;
        }
        timerEnabled = true;
        revealCoroutine = null;
    }

    private void HandleAnswerResult(QuizOptionSO option, bool correct)
    {
        timerEnabled = false;
        if (timerPulseCoroutine != null) { StopCoroutine(timerPulseCoroutine); timerPulseCoroutine = null; }
        SetOptionButtonsInteractable(false);

        int selectedIndex = currentOptions.IndexOf(option);
        StartCoroutine(SuspenseAndReveal(selectedIndex, correct, option));
    }

    private IEnumerator SuspenseAndReveal(int selectedIndex, bool correct, QuizOptionSO option)
    {
        if (selectedIndex >= 0 && selectedIndex < optionButtonImages.Count && optionButtonImages[selectedIndex] != null)
        {
            yield return StartCoroutine(LerpButtonColor(
                optionButtonImages[selectedIndex],
                buttonDefaultColor,
                buttonSelectedColor,
                0.25f));

            yield return StartCoroutine(ScalePunch(optionButtons[selectedIndex].transform,
                                                   optionOriginalScales[selectedIndex], 0.2f, 1.12f));
        }

        yield return new WaitForSeconds(suspenseDuration);

        int correctIndex = -1;
        for (int i = 0; i < currentOptions.Count; i++)
        {
            if (currentOptions[i].isCorrect) correctIndex = i;
        }

        if (correctIndex >= 0 && correctIndex < optionButtonImages.Count && optionButtonImages[correctIndex] != null)
        {
            Color fromColor = (correctIndex == selectedIndex) ? buttonSelectedColor : buttonDefaultColor;
            yield return StartCoroutine(LerpButtonColor(
                optionButtonImages[correctIndex],
                fromColor,
                buttonCorrectColor,
                correctRevealDuration));

            yield return StartCoroutine(ScalePunch(optionButtons[correctIndex].transform,
                                                   optionOriginalScales[correctIndex], 0.3f, 1.15f));
        }

        if (!correct && selectedIndex >= 0 && selectedIndex != correctIndex &&
            selectedIndex < optionButtonImages.Count && optionButtonImages[selectedIndex] != null)
        {
            yield return StartCoroutine(LerpButtonColor(
                optionButtonImages[selectedIndex],
                buttonSelectedColor,
                buttonIncorrectColor,
                correctRevealDuration));
        }

        yield return new WaitForSeconds(0.5f);

        OnResultRevealed?.Invoke(correct);

        if (resultTitleText != null)
        {
            resultTitleText.text = correct ? "¡Correcto!" : "Incorrecto";
            resultTitleText.color = correct ? Color.green : Color.red;
        }
        if (resultBodyText != null)
            resultBodyText.text = correct ? option.correctFeedback : option.incorrectFeedback;

        SetResultPanelActive(true);

        if (resultTitleText != null)
            StartCoroutine(FlashText(resultTitleText, 3, 0.12f));
    }

    private void HandleTimerUpdated(float remaining, float total)
    {
        if (timerSlider != null)
        {
            timerSlider.minValue = 0f;
            timerSlider.maxValue = total;
            timerSlider.value = timerEnabled ? remaining : total;
        }

        if (!timerEnabled) return;

        if (timerLabel != null)
            timerLabel.text = Mathf.CeilToInt(remaining).ToString();

        if (remaining <= timerWarningThreshold)
        {
            if (timerLabel != null) timerLabel.color = timerWarningColor;
            if (timerSlider != null)
            {
                var fillImage = timerSlider.fillRect?.GetComponent<Image>();
                if (fillImage != null) fillImage.color = timerWarningColor;
            }
            if (timerPulseCoroutine == null && timerLabel != null)
                timerPulseCoroutine = StartCoroutine(PulseScale(timerLabel.transform, Vector3.one, 0.5f, 1.2f, true));
        }
    }

    private void HandleTimeOut()
    {
        timerEnabled = false;
        if (timerPulseCoroutine != null) { StopCoroutine(timerPulseCoroutine); timerPulseCoroutine = null; }
        if (timerLabel != null) timerLabel.transform.localScale = Vector3.one;

        SetOptionButtonsInteractable(false);
        SetTimeoutPanelActive(true);
    }

    private void HandleQuizComplete(int correct, int total, float score)
    {
        timerEnabled = false;
        if (timerPulseCoroutine != null) { StopCoroutine(timerPulseCoroutine); timerPulseCoroutine = null; }

        SetResultPanelActive(false);
        SetTimeoutPanelActive(false);
        SetEndPanelActive(true);

        if (endScoreText != null)
            endScoreText.text = $"Respondiste correctamente {correct} de {total} preguntas";

        if (endGradeText != null)
        {
            float rounded = Mathf.Round(score * 10f) / 10f;
            endGradeText.text = $"Tu nota: {rounded:F1} / 10  —  {(score >= LevelGrade.PassingScore ? "Aprobado, pasa al siguiente nivel" : "Desaprobado, sigue intentado que estamos acá para aprender")}";
        }
    }

    // ─── Handlers de botones ──────────────────────────────────────────────────

    private void OnOptionClicked(int index)
    {
        if (index >= currentOptions.Count) return;
        if (!timerEnabled) return;

        audioManager?.PlayClickOption();

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

    // ─── UI helpers ───────────────────────────────────────────────────────────

    private void SetOptionButtonsInteractable(bool value)
    {
        foreach (var btn in optionButtons)
            if (btn != null) btn.interactable = value;
    }

    private void SetResultPanelActive(bool active) { if (resultPanel != null) resultPanel.SetActive(active); }
    private void SetTimeoutPanelActive(bool active) { if (timeoutPanel != null) timeoutPanel.SetActive(active); }
    private void SetEndPanelActive(bool active) { if (endPanel != null) endPanel.SetActive(active); }

    private void ResetAllOptionsToDefault()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (optionButtons[i] != null)
                optionButtons[i].transform.localScale = optionOriginalScales[i];

            if (i < optionButtonImages.Count && optionButtonImages[i] != null)
                optionButtonImages[i].color = buttonDefaultColor;
        }

        if (timerLabel != null)
        {
            timerLabel.color = timerLabelOriginalColor;
            timerLabel.transform.localScale = Vector3.one;
        }
        if (timerSlider != null)
        {
            var fillImage = timerSlider.fillRect?.GetComponent<Image>();
            if (fillImage != null) fillImage.color = timerNormalColor;
        }

        if (situationText != null)
            situationText.color = new Color(situationOriginalColor.r, situationOriginalColor.g,
                                            situationOriginalColor.b, 1f);
    }

    // ─── Coroutines de animación ──────────────────────────────────────────────

    private IEnumerator FadeTextAlpha(TMP_Text text, float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = text.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);
            c.a = Mathf.Lerp(from, to, t);
            text.color = c;
            yield return null;
        }
        c.a = to;
        text.color = c;
    }

    private IEnumerator ScalePunchReveal(Transform target, Vector3 finalScale, float duration)
    {
        float overshoot = 1.15f;
        float halfTime = duration * 0.65f;
        float elapsed = 0f;

        while (elapsed < halfTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfTime);
            t = 1f - Mathf.Pow(1f - t, 3f);
            target.localScale = Vector3.LerpUnclamped(Vector3.zero, finalScale * overshoot, t);
            yield return null;
        }

        float secondPhase = duration - halfTime;
        elapsed = 0f;
        while (elapsed < secondPhase)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / secondPhase);
            t = t * t * (3f - 2f * t);
            target.localScale = Vector3.LerpUnclamped(finalScale * overshoot, finalScale, t);
            yield return null;
        }
        target.localScale = finalScale;
    }

    private IEnumerator ScalePunch(Transform target, Vector3 baseScale, float duration, float peakMultiplier = 1.15f)
    {
        float half = duration * 0.5f;
        float elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            target.localScale = Vector3.LerpUnclamped(baseScale, baseScale * peakMultiplier, t);
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            target.localScale = Vector3.LerpUnclamped(baseScale * peakMultiplier, baseScale, t);
            yield return null;
        }
        target.localScale = baseScale;
    }

    private IEnumerator PulseScale(Transform target, Vector3 baseScale, float duration,
                                   float peakMultiplier = 1.2f, bool loop = false)
    {
        do
        {
            yield return StartCoroutine(ScalePunch(target, baseScale, duration, peakMultiplier));
            yield return new WaitForSeconds(0.05f);
        } while (loop);
    }

    private IEnumerator LerpButtonColor(Image image, Color from, Color to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);
            image.color = Color.Lerp(from, to, t);
            yield return null;
        }
        image.color = to;
    }

    private IEnumerator FlashText(TMP_Text text, int times, float halfPeriod)
    {
        Color original = text.color;
        for (int i = 0; i < times; i++)
        {
            Color c = text.color;
            c.a = 0f;
            text.color = c;
            yield return new WaitForSeconds(halfPeriod);
            c.a = 1f;
            text.color = c;
            yield return new WaitForSeconds(halfPeriod);
        }
        text.color = original;
    }
}