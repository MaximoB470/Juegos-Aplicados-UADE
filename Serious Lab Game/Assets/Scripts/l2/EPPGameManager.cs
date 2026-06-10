using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager de lógica del Nivel 2 (EPP - vestimenta correcta).
/// Implementa ILevelScorer con crédito parcial por categoría:
///   nota = (categorías correctas / categorías posibles) * 10
/// Esto asegura que agregar situaciones o categorías no rompa el cálculo.
/// </summary>
public class EPPGameManager : MonoBehaviour, ILevelScorer
{
    private const string SERVICE_KEY = "EPPGameManager";

    [Header("Identificación de nivel")]
    [Tooltip("Debe coincidir con el índice del nodo en LevelSelectorManager (base 0).")]
    [SerializeField] private int levelIndex = 1;

    [Header("Escenarios del nivel (en orden)")]
    [SerializeField] private List<EPPScenarioSO> scenarios;

    private int currentScenarioIndex     = 0;
    private int totalCorrectCategories   = 0;
    private int totalPossibleCategories  = 0;

    // ─── ILevelScorer ─────────────────────────────────────────────────────────

    public int LevelIndex => levelIndex;

    /// <summary>
    /// Nota = (categorías correctas acumuladas / categorías posibles acumuladas) * 10.
    /// Crédito parcial: acertar cabeza y cuerpo pero no manos ni pies
    /// da una nota proporcional, no cero.
    /// </summary>
    public float CalculateScore()
    {
        if (totalPossibleCategories == 0) return 0f;
        return ((float)totalCorrectCategories / totalPossibleCategories) * 10f;
    }

    // ─── Eventos ──────────────────────────────────────────────────────────────

    public event Action<EPPScenarioSO>  OnScenarioLoaded;
    public event Action<EPPResult>      OnResultReady;

    /// <summary>correct, total, score 0-10</summary>
    public event Action<int, int, float> OnLevelComplete;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        ServiceLocator.Instance.SetService(SERVICE_KEY, this);
    }

    private void Start()
    {
        Time.timeScale = 1f;
        LoadCurrentScenario();
    }

    private void OnDestroy()
    {
        OnScenarioLoaded = null;
        OnResultReady    = null;
        OnLevelComplete  = null;
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    public void SubmitAnswer(int headIndex, int bodyIndex, int handsIndex, int feetIndex)
    {
        if (scenarios == null || currentScenarioIndex >= scenarios.Count) return;

        EPPScenarioSO scenario = scenarios[currentScenarioIndex];
        EPPResult result = EvaluateAnswer(scenario, headIndex, bodyIndex, handsIndex, feetIndex);

        OnResultReady?.Invoke(result);
    }

    public void AdvanceToNextScenario()
    {
        currentScenarioIndex++;

        if (currentScenarioIndex < scenarios.Count)
        {
            LoadCurrentScenario();
        }
        else
        {
            float score = CalculateScore();
            SubmitGrade(score);
            OnLevelComplete?.Invoke(totalCorrectCategories, totalPossibleCategories, score);
        }
    }

    // ─── Privados ─────────────────────────────────────────────────────────────

    private void LoadCurrentScenario()
    {
        if (scenarios == null || scenarios.Count == 0)
        {
            Debug.LogWarning("[EPPGameManager] La lista de escenarios está vacía.");
            return;
        }

        OnScenarioLoaded?.Invoke(scenarios[currentScenarioIndex]);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            DebugWin();
        }
    }
    private void DebugWin()
    {
        const float debugScore = 10f;

        SubmitGrade(debugScore);

        Debug.Log("[DEBUG] EPP completado con nota 10.");

        OnLevelComplete?.Invoke(1, 1, debugScore);
    }
    private EPPResult EvaluateAnswer(
        EPPScenarioSO scenario,
        int headIndex, int bodyIndex, int handsIndex, int feetIndex)
    {
        var result = new EPPResult();

        result.headCorrect  = EvaluateCategory(scenario.headOptions,  headIndex,  "Cabeza", result);
        result.bodyCorrect  = EvaluateCategory(scenario.bodyOptions,  bodyIndex,  "Cuerpo", result);
        result.handsCorrect = EvaluateCategory(scenario.handsOptions, handsIndex, "Manos",  result);
        result.feetCorrect  = EvaluateCategory(scenario.feetOptions,  feetIndex,  "Pies",   result);

        // Acumular crédito parcial por categoría
        AccumulateCategories(scenario, result);

        result.scenarioFeedback = result.allCorrect
            ? scenario.feedbackCorrect
            : scenario.feedbackIncorrect;

        return result;
    }

    private bool EvaluateCategory(
        List<EPPOptionSO> options,
        int selectedIndex,
        string categoryName,
        EPPResult result)
    {
        if (options == null || options.Count == 0) return true;

        int safeIndex     = Mathf.Clamp(selectedIndex, 0, options.Count - 1);
        EPPOptionSO chosen = options[safeIndex];

        if (chosen.isCorrect) return true;

        result.incorrectLabels.Add(chosen.optionLabel);
        result.incorrectCategoryNames.Add(categoryName);

        EPPOptionSO correctOption = options.Find(o => o.isCorrect);
        result.correctLabels.Add(correctOption != null ? correctOption.optionLabel : "—");

        return false;
    }

    /// <summary>
    /// Suma al acumulador global solo las categorías no vacías.
    /// Así agregar o quitar categorías no rompe el cálculo.
    /// </summary>
    private void AccumulateCategories(EPPScenarioSO scenario, EPPResult result)
    {
        CountCategory(scenario.headOptions,  result.headCorrect);
        CountCategory(scenario.bodyOptions,  result.bodyCorrect);
        CountCategory(scenario.handsOptions, result.handsCorrect);
        CountCategory(scenario.feetOptions,  result.feetCorrect);
    }

    private void CountCategory(List<EPPOptionSO> options, bool isCorrect)
    {
        if (options == null || options.Count == 0) return;
        totalPossibleCategories++;
        if (isCorrect) totalCorrectCategories++;
    }

    private void SubmitGrade(float score)
    {
        var gradeService = ServiceLocator.Instance.GetService("GradeService") as GradeService;
        gradeService?.SubmitGrade(levelIndex, score);
    }
}
