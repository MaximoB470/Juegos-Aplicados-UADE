using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manager de lógica del Nivel 1 (encontrar fallas).
/// Implementa ILevelScorer: calcula la nota dinámicamente
/// en base a fallas encontradas / fallas totales existentes.
/// </summary>
public class GameManager : MonoBehaviour, ILevelScorer
{
    public static GameManager Instance { get; private set; }

    [Header("Identificación de nivel")]
    [Tooltip("Debe coincidir con el índice del nodo en LevelSelectorManager (base 0).")]
    [SerializeField] private int levelIndex = 0;

    [Header("Click Points")]
    [SerializeField] private List<ClickPoint> allClickPoints;

    [Header("Timer (opcional)")]
    [SerializeField] private bool  useTimer     = false;
    [SerializeField] private float gameDuration = 180f;

    [Header("Límite de Clicks incorrectos")]
    [Tooltip("0 = sin límite.")]
    [SerializeField] private int maxWrongClicks = 10;

    [Header("Límite de Pistas")]
    [SerializeField] private int maxHints = 3;

    private int   foundCount      = 0;
    private int   wrongClickCount = 0;
    private int   hintsUsed       = 0;
    private float currentTime;
    private bool  gameRunning     = false;

    // ─── ILevelScorer ─────────────────────────────────────────────────────────

    public int LevelIndex => levelIndex;

    /// <summary>
    /// Nota = (fallas encontradas / total de fallas) * 10.
    /// Si no hay fallas configuradas devuelve 0.
    /// </summary>
    public float CalculateScore()
    {
        if (TotalPoints == 0) return 0f;
        return ((float)foundCount / TotalPoints) * 10f;
    }

    // ─── Propiedades ──────────────────────────────────────────────────────────

    public int   TotalPoints     => allClickPoints != null ? allClickPoints.Count : 0;
    public int   FoundCount      => foundCount;
    public float RemainingTime   => Mathf.Max(currentTime, 0f);
    public int   WrongClickCount => wrongClickCount;
    public int   MaxWrongClicks  => maxWrongClicks;
    public int   HintsUsed       => hintsUsed;
    public int   MaxHints        => maxHints;

    private UIManager GetUI() => UIManager.Instance;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()  => StartGame();

    private void Update()
    {
        if (!gameRunning || !useTimer || GetUI().IsPaused) return;

        currentTime -= Time.deltaTime;
        GetUI().UpdateTimerDisplay(currentTime);
        if (currentTime <= 0f) LoseGame();

        if (Input.GetKeyDown(KeyCode.F1)) LoseGame();
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    public void StartGame()
    {
        foundCount       = 0;
        wrongClickCount  = 0;
        hintsUsed        = 0;
        currentTime      = gameDuration;
        gameRunning      = true;

        foreach (var cp in allClickPoints)
            cp.ResetPoint();

        GetUI().HideAll();
        GetUI().ResumeGame();
        GetUI().UpdateProgressDisplay(foundCount, TotalPoints);
        GetUI().UpdateClickDisplay(wrongClickCount, maxWrongClicks);
        GetUI().UpdateHintButton(hintsUsed, maxHints);
    }

    public void OnPointClicked(ClickPoint point)
    {
        if (!gameRunning || GetUI().IsPaused) return;
        if (point.IsFound) return;
        GetUI().ShowInfoPanel(point);
    }

    public void RegisterWrongClick()
    {
        if (!gameRunning || GetUI().IsPaused) return;
        if (maxWrongClicks <= 0) return;

        wrongClickCount++;
        GetUI().UpdateClickDisplay(wrongClickCount, maxWrongClicks);

        if (wrongClickCount >= maxWrongClicks)
            LoseGame();
    }

    public void RegisterPointFound()
    {
        foundCount++;
        GetUI().UpdateProgressDisplay(foundCount, TotalPoints);

        if (foundCount >= TotalPoints)
            Invoke(nameof(WinGame), 0.15f);
    }

    public void ShowHint()
    {
        if (!gameRunning || GetUI().IsPaused) return;
        if (hintsUsed >= maxHints) return;

        var remaining = new List<ClickPoint>();
        foreach (var cp in allClickPoints)
            if (!cp.IsFound) remaining.Add(cp);

        if (remaining.Count > 0)
        {
            hintsUsed++;
            GetUI().ShowHintPanel(remaining[Random.Range(0, remaining.Count)]);
            GetUI().UpdateHintButton(hintsUsed, maxHints);
        }
    }

    // ─── Fin de partida ───────────────────────────────────────────────────────

    private void WinGame()
    {
        gameRunning  = false;
        float score  = CalculateScore();
        Debug.Log($"[GameManager] WinGame! Score calculado: {score}");
        SubmitGrade(score);
        GetUI().ShowWin(score);
    }

    private void LoseGame()
    {
        gameRunning  = false;
        float score  = CalculateScore();
        Debug.Log($"[GameManager] LoseGame! Score calculado: {score}");
        SubmitGrade(score);
        GetUI().ShowLose();
    }

    private void SubmitGrade(float score)
    {
        var gradeService = ServiceLocator.Instance.GetService("GradeService") as GradeService;
        if (gradeService == null)
        {
            Debug.LogError($"[GameManager] CRÍTICO: No se encontró GradeService en el ServiceLocator para enviar la nota del nivel {levelIndex}.");
        }
        else
        {
            Debug.Log($"[GameManager] GradeService encontrado. Enviando nota {score} para el nivel {levelIndex}.");
        }
        
        gradeService?.SubmitGrade(levelIndex, score);
    }
}
