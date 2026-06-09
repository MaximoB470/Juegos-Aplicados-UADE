using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manager de UI del Nivel 3 (Quiz) — estilo ¿Quién quiere ser millonario?
/// Todas las animaciones son por script (Coroutines).
/// Solo presenta datos y delega acciones al QuizGameManager.
/// </summary>
public class QuizUIManager : MonoBehaviour
{
    private const string SERVICE_KEY = "QuizUIManager";
    private const string GAME_MANAGER_KEY = "QuizGameManager";
    private const string SELECTOR_SCENE = "LevelSelector";

    private QuizGameManager gameManager;

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

    [SerializeField] private Color buttonDefaultColor = new Color(0.15f, 0.15f, 0.25f, 1f); // ajustá al color real de tus botones
    [SerializeField] private Color buttonSelectedColor = new Color(1f, 0.85f, 0.1f, 1f); // amarillo (suspense)
    [SerializeField] private Color buttonCorrectColor = new Color(0.1f, 0.85f, 0.3f, 1f); // verde
    [SerializeField] private Color buttonIncorrectColor = new Color(0.85f, 0.1f, 0.1f, 1f); // rojo
    [SerializeField] private Color timerNormalColor = Color.white;
    [SerializeField] private Color timerWarningColor = new Color(1f, 0.35f, 0.1f, 1f);

    // Imágenes de los botones para colorear (CanvasRenderer / Image)
    private List<Image> optionButtonImages = new();

    // Valores originales para restaurar
    private List<Vector3> optionOriginalScales = new();
    private Color situationOriginalColor;
    private Color timerLabelOriginalColor;

    private List<QuizOptionSO> currentOptions = new();
    private bool timerEnabled = false; // el timer solo corre después de que aparecen las opciones

    // Coroutine handles para cancelar si hace falta
    private Coroutine timerPulseCoroutine;
    private Coroutine revealCoroutine;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        ServiceLocator.Instance.SetService(SERVICE_KEY, this);

        // Cachear imágenes de botones y escalas originales
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

        // Colores originales del texto
        if (situationText != null)
            situationOriginalColor = situationText.color;

        if (timerLabel != null)
            timerLabelOriginalColor = timerLabel.color;
    }

    private void Start()
    {
        gameManager = ServiceLocator.Instance.GetService(GAME_MANAGER_KEY) as QuizGameManager;

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
        // Cancelar coroutines anteriores
        if (revealCoroutine != null) StopCoroutine(revealCoroutine);
        if (timerPulseCoroutine != null) StopCoroutine(timerPulseCoroutine);

        timerEnabled = false; // el timer no corre hasta que terminen las animaciones

        SetResultPanelActive(false);
        SetTimeoutPanelActive(false);

        // Resetear todos los botones a estado original antes de la nueva ronda
        ResetAllOptionsToDefault();

        // Resetear timer UI
        if (timerSlider != null)
        {
            timerSlider.value = timerSlider.maxValue;
        }
        if (timerLabel != null)
        {
            timerLabel.color = timerLabelOriginalColor;
        }

        currentOptions = situation.options;
        revealCoroutine = StartCoroutine(RevealSituationSequence(situation));
    }

    /// <summary>
    /// Secuencia principal: fade texto → pausa lectura → aparecen opciones una a una → timer ON
    /// </summary>
    private IEnumerator RevealSituationSequence(QuizSituationSO situation)
    {
        // 1. Ocultar opciones (escala 0 o alpha 0 para animarlas)
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (optionButtons[i] != null)
            {
                bool hasOption = i < currentOptions.Count;
                optionButtons[i].gameObject.SetActive(hasOption);
                optionButtons[i].interactable = false;
                if (hasOption)
                {
                    // Empezar con escala 0 para el punch de entrada
                    optionButtons[i].transform.localScale = Vector3.zero;
                }
            }
        }

        // 2. Fade IN del texto de situación
        if (situationText != null)
        {
            situationText.text = situation.situationText;
            situationText.color = new Color(situationOriginalColor.r, situationOriginalColor.g,
                                            situationOriginalColor.b, 0f);
            yield return StartCoroutine(FadeTextAlpha(situationText, 0f, 1f, situationFadeDuration));
        }

        // 3. Pausa de lectura
        yield return new WaitForSeconds(readingPause);

        // 4. Opciones aparecen una por una con ScalePunch
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (i >= currentOptions.Count) break;

            if (optionLabels != null && i < optionLabels.Count && optionLabels[i] != null)
                optionLabels[i].text = currentOptions[i].optionText;

            // Restablecer color del botón
            if (i < optionButtonImages.Count && optionButtonImages[i] != null)
                optionButtonImages[i].color = buttonDefaultColor;

            yield return StartCoroutine(ScalePunchReveal(
                optionButtons[i].transform,
                optionOriginalScales[i],
                optionRevealDuration));

            yield return new WaitForSeconds(optionStaggerDelay);
        }

        // 5. Pequeña pausa extra antes de habilitar interacción y timer
        yield return new WaitForSeconds(0.15f);

        // 6. Habilitar botones y activar timer
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

    /// <summary>
    /// 1. Pulsar el botón elegido en amarillo (suspense) → esperar → revelar correcta en verde.
    /// </summary>
    private IEnumerator SuspenseAndReveal(int selectedIndex, bool correct, QuizOptionSO option)
    {
        // Paso 1: Iluminar el seleccionado en amarillo
        if (selectedIndex >= 0 && selectedIndex < optionButtonImages.Count && optionButtonImages[selectedIndex] != null)
        {
            yield return StartCoroutine(LerpButtonColor(
                optionButtonImages[selectedIndex],
                buttonDefaultColor,
                buttonSelectedColor,
                0.25f));

            // Pequeño pulso de escala en el seleccionado
            yield return StartCoroutine(ScalePunch(optionButtons[selectedIndex].transform,
                                                   optionOriginalScales[selectedIndex], 0.2f, 1.12f));
        }

        // Paso 2: Suspenso
        yield return new WaitForSeconds(suspenseDuration);

        // Paso 3: Revelar la correcta en verde (y la incorrecta en rojo si la eligió)
        int correctIndex = -1;
        for (int i = 0; i < currentOptions.Count; i++)
        {
            if (currentOptions[i].isCorrect) correctIndex = i;
        }

        // Colorear la correcta en verde con animación
        if (correctIndex >= 0 && correctIndex < optionButtonImages.Count && optionButtonImages[correctIndex] != null)
        {
            Color fromColor = (correctIndex == selectedIndex) ? buttonSelectedColor : buttonDefaultColor;
            yield return StartCoroutine(LerpButtonColor(
                optionButtonImages[correctIndex],
                fromColor,
                buttonCorrectColor,
                correctRevealDuration));

            // Punch de escala en la correcta
            yield return StartCoroutine(ScalePunch(optionButtons[correctIndex].transform,
                                                   optionOriginalScales[correctIndex], 0.3f, 1.15f));
        }

        // Si eligió incorrecta, pintarla en rojo
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

        // Paso 4: Mostrar result panel con datos
        if (resultTitleText != null)
        {
            resultTitleText.text = correct ? "¡Correcto!" : "Incorrecto";
            resultTitleText.color = correct ? Color.green : Color.red;
        }
        if (resultBodyText != null)
            resultBodyText.text = correct ? option.correctFeedback : option.incorrectFeedback;

        SetResultPanelActive(true);

        // Flash del título del resultado
        if (resultTitleText != null)
            StartCoroutine(FlashText(resultTitleText, 3, 0.12f));
    }

    private void HandleTimerUpdated(float remaining, float total)
    {
        // El slider siempre se actualiza (el GameManager lo manda), pero solo
        // dejamos que cuente visualmente cuando timerEnabled es true.
        if (timerSlider != null)
        {
            timerSlider.minValue = 0f;
            timerSlider.maxValue = total;
            timerSlider.value = timerEnabled ? remaining : total;
        }

        if (!timerEnabled) return;

        if (timerLabel != null)
            timerLabel.text = Mathf.CeilToInt(remaining).ToString();

        // Advertencia cuando queda poco tiempo
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

    /// <summary>correct, total, score — nota ya reportada a GradeService por QuizGameManager.</summary>
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
            endGradeText.text = $"Tu nota: {rounded:F1} / 10  —  {(score >= LevelGrade.PassingScore ? "Aprobado ✓" : "Desaprobado ✗")}";
        }
    }

    // ─── Handlers de botones ──────────────────────────────────────────────────

    private void OnOptionClicked(int index)
    {
        if (index >= currentOptions.Count) return;
        if (!timerEnabled) return; // evitar doble click durante animaciones
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

    /// <summary>
    /// Vuelve todos los botones a su estado visual original (color, escala, interactable).
    /// Se llama al cargar cada nueva situación.
    /// </summary>
    private void ResetAllOptionsToDefault()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (optionButtons[i] != null)
                optionButtons[i].transform.localScale = optionOriginalScales[i];

            if (i < optionButtonImages.Count && optionButtonImages[i] != null)
                optionButtonImages[i].color = buttonDefaultColor;
        }

        // Resetear color del timer
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

        // Restaurar alpha del situation text
        if (situationText != null)
            situationText.color = new Color(situationOriginalColor.r, situationOriginalColor.g,
                                            situationOriginalColor.b, 1f);
    }

    // ─── Coroutines de animación ──────────────────────────────────────────────

    /// <summary>Fade del alpha de un TMP_Text de 'from' a 'to' en 'duration' segundos.</summary>
    private IEnumerator FadeTextAlpha(TMP_Text text, float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = text.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Ease in-out suave
            t = t * t * (3f - 2f * t);
            c.a = Mathf.Lerp(from, to, t);
            text.color = c;
            yield return null;
        }
        c.a = to;
        text.color = c;
    }

    /// <summary>
    /// Revela un objeto con un punch de escala: va de 0 → overshoot → escala original.
    /// </summary>
    private IEnumerator ScalePunchReveal(Transform target, Vector3 finalScale, float duration)
    {
        float overshoot = 1.15f;
        float halfTime = duration * 0.65f;
        float elapsed = 0f;

        // Fase 1: 0 → finalScale * overshoot
        while (elapsed < halfTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfTime);
            t = 1f - Mathf.Pow(1f - t, 3f); // ease out cubic
            target.localScale = Vector3.LerpUnclamped(Vector3.zero, finalScale * overshoot, t);
            yield return null;
        }

        // Fase 2: overshoot → finalScale
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

    /// <summary>
    /// Pequeño punch de escala sobre la escala actual (sin partir de cero).
    /// </summary>
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

    /// <summary>
    /// Pulso continuo de escala (para el timer en modo warning).
    /// Si loop=true corre indefinidamente hasta que se cancele la coroutine.
    /// </summary>
    private IEnumerator PulseScale(Transform target, Vector3 baseScale, float duration,
                                   float peakMultiplier = 1.2f, bool loop = false)
    {
        do
        {
            yield return StartCoroutine(ScalePunch(target, baseScale, duration, peakMultiplier));
            yield return new WaitForSeconds(0.05f);
        } while (loop);
    }

    /// <summary>Lerp de color de una Image de A a B en 'duration' segundos.</summary>
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

    /// <summary>Flash de alpha en un TMP_Text (parpadeo rápido). Útil para el título de resultado.</summary>
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
