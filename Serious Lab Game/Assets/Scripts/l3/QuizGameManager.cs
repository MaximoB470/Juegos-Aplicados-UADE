using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager de lógica del Nivel 3 (Quiz).
/// Implementa ILevelScorer:
///   nota = (respuestas correctas / total de preguntas) * 10
/// </summary>
public class QuizGameManager : MonoBehaviour, ILevelScorer
{
    private const string SERVICE_KEY = "QuizGameManager";

    [Header("Identificación de nivel")]
    [Tooltip("Debe coincidir con el índice del nodo en LevelSelectorManager (base 0).")]
    [SerializeField] private int levelIndex = 2;

    [Header("Preguntas del quiz (en orden)")]
    [SerializeField] private List<QuizSituationSO> situations;

    [Header("Timer por pregunta")]
    [SerializeField] private float timePerQuestion = 15f;

    private int   currentIndex   = 0;
    private int   correctCount   = 0;
    private float currentTime    = 0f;
    private bool  questionActive = false;

    // ─── ILevelScorer ─────────────────────────────────────────────────────────

    public int LevelIndex => levelIndex;

    /// <summary>
    /// Nota = (respuestas correctas / total de preguntas) * 10.
    /// Las preguntas no respondidas (timeout) cuentan como incorrectas.
    /// </summary>
    public float CalculateScore()
    {
        if (situations == null || situations.Count == 0) return 0f;
        return ((float)correctCount / situations.Count) * 10f;
    }

    // ─── Eventos ──────────────────────────────────────────────────────────────

    public event Action<QuizSituationSO>   OnSituationLoaded;
    public event Action<QuizOptionSO, bool> OnAnswerResult;
    public event Action<float, float>      OnTimerUpdated;
    public event Action                    OnTimeOut;

    /// <summary>correct, total, score 0-10</summary>
    public event Action<int, int, float>   OnQuizComplete;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        ServiceLocator.Instance.SetService(SERVICE_KEY, this);
    }

    private void Start()
    {
        Time.timeScale = 1f;
        
        LoadCurrentSituation();
    }

    private void Update()
    {
        if (questionActive)
        {
            currentTime -= Time.deltaTime;
            OnTimerUpdated?.Invoke(currentTime, timePerQuestion);

            if (currentTime <= 0f)
            {
                questionActive = false;
                OnTimeOut?.Invoke();
            }
        }

        if (Input.GetKeyDown(KeyCode.F12))
        {
            DebugWin();
        }
    }

    private void DebugWin()
    {
        questionActive = false;

        const float debugScore = 10f;

        SubmitGrade(debugScore);

        Debug.Log("[DEBUG] Quiz completado con nota 10.");

        OnQuizComplete?.Invoke(1, 1, debugScore);
    }

    private void OnDestroy()
    {
        OnSituationLoaded = null;
        OnAnswerResult    = null;
        OnTimerUpdated    = null;
        OnTimeOut         = null;
        OnQuizComplete    = null;
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    public void SubmitAnswer(QuizOptionSO option)
    {
        if (!questionActive) return;

        questionActive = false;

        if (option.isCorrect) correctCount++;

        OnAnswerResult?.Invoke(option, option.isCorrect);
    }

    public void AdvanceToNext()
    {
        currentIndex++;

        if (currentIndex < situations.Count)
        {
            LoadCurrentSituation();
        }
        else
        {
            float score = CalculateScore();
            SubmitGrade(score);
            OnQuizComplete?.Invoke(correctCount, situations.Count, score);
        }
    }

    // ─── Privados ─────────────────────────────────────────────────────────────

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

    private void SubmitGrade(float score)
    {
        var gradeService = ServiceLocator.Instance.GetService("GradeService") as GradeService;
        gradeService?.SubmitGrade(levelIndex, score);
    }
}
