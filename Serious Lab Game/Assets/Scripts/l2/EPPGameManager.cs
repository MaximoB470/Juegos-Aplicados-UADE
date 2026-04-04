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
    private int totalScore           = 0;   // puntos acumulados (1 por escenario correcto)
    private int correctCount         = 0;   // escenarios completados sin errores

    /// <summary>Se dispara al cargar un nuevo escenario.</summary>
    public event Action<EPPScenarioSO> OnScenarioLoaded;

    /// <summary>Se dispara con el resultado evaluado cuando el jugador confirma.</summary>
    public event Action<EPPResult> OnResultReady;

    /// <summary>
    /// Se dispara al completar todos los escenarios.
    /// Parámetros: (escenarios correctos, total de escenarios).
    /// </summary>
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
        // Limpieza defensiva: los suscriptores deben desuscribirse en su propio
        // OnDestroy, pero nulificamos los eventos para liberar cualquier
        // referencia remanente.
        OnScenarioLoaded  = null;
        OnResultReady     = null;
        OnLevelComplete   = null;
    }

    /// <summary>
    /// Evalúa la selección del jugador y dispara <see cref="OnResultReady"/>.
    /// Llamado por EPPUIManager desde el botón "Confirmar".
    /// </summary>
    /// <param name="headIndex">Índice seleccionado en el slider de Cabeza.</param>
    /// <param name="bodyIndex">Índice seleccionado en el slider de Cuerpo.</param>
    /// <param name="handsIndex">Índice seleccionado en el slider de Manos.</param>
    /// <param name="feetIndex">Índice seleccionado en el slider de Pies.</param>
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

    /// <summary>
    /// Avanza al siguiente escenario o dispara <see cref="OnLevelComplete"/>.
    /// Llamado por EPPUIManager desde el botón "Continuar" del panel de resultado.
    /// </summary>
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

    /// <summary>
    /// Evalúa cada categoría contra el isCorrect del EPPOptionSO correspondiente
    /// y construye un EPPResult completo.
    /// </summary>
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

    /// <summary>
    /// Evalúa una sola categoría. Si es incorrecta, registra los labels en el
    /// EPPResult. Devuelve true si la opción elegida es correcta.
    /// </summary>
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
