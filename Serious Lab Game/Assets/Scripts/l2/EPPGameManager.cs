using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager de lógica del nivel EPP.
/// - Se registra en el ServiceLocator con la clave "EPPGameManager".
/// - No tiene ninguna referencia directa a elementos de UI.
/// - Comunica cambios de estado a través de eventos de C#.
/// </summary>
public class EPPGameManager : MonoBehaviour
{
    private const string SERVICE_KEY = "EPPGameManager";

    [Header("Escenarios del nivel (en orden)")]
    [SerializeField] private List<EPPScenarioSO> scenarios;

    private int currentScenarioIndex = 0;
    private int totalScore           = 0;  
    private int correctCount         = 0;   

   
    public event Action<EPPScenarioSO> OnScenarioLoaded;
    public event Action<EPPResult> OnResultReady;
    public event Action<int, int> OnLevelComplete;

    private void Awake()
    {
        ServiceLocator.Instance.SetService(SERVICE_KEY, this);
    }

    private void Start()
    {
        LoadCurrentScenario();
    }

    private void OnDestroy()
    {
        OnScenarioLoaded  = null;
        OnResultReady     = null;
        OnLevelComplete   = null;
    }

    public void SubmitAnswer(int headIndex, int bodyIndex, int handsIndex, int feetIndex)
    {
        if (scenarios == null || currentScenarioIndex >= scenarios.Count) return;

        EPPScenarioSO scenario = scenarios[currentScenarioIndex];
        EPPResult result = EvaluateAnswer(scenario, headIndex, bodyIndex, handsIndex, feetIndex);

        if (result.allCorrect)
        {
            correctCount++;
            totalScore++;
        }

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
            OnLevelComplete?.Invoke(correctCount, scenarios.Count);
        }
    }

    private void LoadCurrentScenario()
    {
        if (scenarios == null || scenarios.Count == 0)
        {
            Debug.LogWarning("[EPPGameManager] La lista de escenarios está vacía.");
            return;
        }

        EPPScenarioSO scenario = scenarios[currentScenarioIndex];
        OnScenarioLoaded?.Invoke(scenario);
    }

    private EPPResult EvaluateAnswer(
        EPPScenarioSO scenario,
        int headIndex, int bodyIndex, int handsIndex, int feetIndex)
    {
        var result = new EPPResult();

        result.headCorrect  = EvaluateCategory(
            scenario.headOptions,  headIndex,  "Cabeza",
            result);

        result.bodyCorrect  = EvaluateCategory(
            scenario.bodyOptions,  bodyIndex,  "Cuerpo",
            result);

        result.handsCorrect = EvaluateCategory(
            scenario.handsOptions, handsIndex, "Manos",
            result);

        result.feetCorrect  = EvaluateCategory(
            scenario.feetOptions,  feetIndex,  "Pies",
            result);

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
        if (options == null || options.Count == 0)
        {
            Debug.LogWarning($"[EPPGameManager] Categoría '{categoryName}' sin opciones.");
            return true; 
        }

        int safeIndex = Mathf.Clamp(selectedIndex, 0, options.Count - 1);
        EPPOptionSO chosen = options[safeIndex];

        if (chosen.isCorrect) return true;

        result.incorrectLabels.Add(chosen.optionLabel);
        result.incorrectCategoryNames.Add(categoryName);

        EPPOptionSO correctOption = options.Find(o => o.isCorrect);
        result.correctLabels.Add(correctOption != null ? correctOption.optionLabel : "—");

        return false;
    }
}
