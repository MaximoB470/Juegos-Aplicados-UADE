using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager de lógica del nivel Quiz (Nivel 3).
/// - Se registra en el ServiceLocator con la clave "QuizGameManager".
/// - No tiene referencias directas a UI.
/// - Comunica cambios de estado a través de eventos de C#.
/// </summary>
public class QuizGameManager : MonoBehaviour
{
    private const string SERVICE_KEY = "QuizGameManager";

    [Header("Preguntas del quiz (en orden)")]
    [SerializeField] private List<QuizSituationSO> situations;

    [Header("Timer por pregunta")]
    [SerializeField] private float timePerQuestion = 15f;

    private int  currentIndex  = 0;
    private int  correctCount  = 0;
    private float currentTime  = 0f;
    private bool questionActive = false;

    // ─── Eventos ─────────────────────────────────────────────────────────────

    /// <summary>Nueva situación cargada — la UI sobreescribe textos con los datos.</summary>
    public event Action<QuizSituationSO> OnSituationLoaded;

    /// <summary>Resultado de una respuesta elegida por el jugador.</summary>
    public event Action<QuizOptionSO, bool> OnAnswerResult;

    /// <summary>Timer actualizado — float: tiempo restante, float: tiempo total.</summary>
    public event Action<float, float> OnTimerUpdated;

    /// <summary>Tiempo agotado sin respuesta.</summary>
    public event Action OnTimeOut;

    /// <summary>Quiz completo — int: correctas, int: total.</summary>
    public event Action<int, int> OnQuizComplete;

    // ─── Lifecycle ───────────────────────────────────────────────────────────

    private void Awake()
    {
        ServiceLocator.Instance.SetService(SERVICE_KEY, this);
    }

    private void Start()
    {
        LoadCurrentSituation();
    }

    private void Update()
    {
        if (!questionActive) return;

        currentTime -= Time.deltaTime;
        OnTimerUpdated?.Invoke(currentTime, timePerQuestion);

        if (currentTime <= 0f)
        {
            questionActive = false;
            OnTimeOut?.Invoke();
        }
    }

    private void OnDestroy()
    {
        OnSituationLoaded = null;
        OnAnswerResult    = null;
        OnTimerUpdated    = null;
        OnTimeOut         = null;
        OnQuizComplete    = null;
    }

    // ─── API pública ─────────────────────────────────────────────────────────

    /// <summary>
    /// Llamado por la UI cuando el jugador elige una opción.
    /// </summary>
    public void SubmitAnswer(QuizOptionSO option)
    {
        if (!questionActive) return;

        questionActive = false;

        if (option.isCorrect)
            correctCount++;

        OnAnswerResult?.Invoke(option, option.isCorrect);
    }

    /// <summary>
    /// Llamado por la UI al cerrar el panel de resultado o al terminar el timeout,
    /// avanza a la siguiente pregunta o termina el quiz.
    /// </summary>
    public void AdvanceToNext()
    {
        currentIndex++;

        if (currentIndex < situations.Count)
            LoadCurrentSituation();
        else
            OnQuizComplete?.Invoke(correctCount, situations.Count);
    }

    // ─── Privados ────────────────────────────────────────────────────────────

    private void LoadCurrentSituation()
    {
        if (situations == null || situations.Count == 0)
        {
            Debug.LogWarning("[QuizGameManager] La lista de situaciones está vacía.");
            return;
        }

        currentTime    = timePerQuestion;
        questionActive = true;

        OnSituationLoaded?.Invoke(situations[currentIndex]);
        OnTimerUpdated?.Invoke(currentTime, timePerQuestion);
    }
}
