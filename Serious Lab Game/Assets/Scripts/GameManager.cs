using System.Collections;
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
    [SerializeField] private bool useTimer = false;
    [SerializeField] private float gameDuration = 180f;
    [SerializeField] private float timeLimit = 120f;

    [Header("Límite de Clicks incorrectos")]
    [Tooltip("0 = sin límite. Se reinicia cada vez que se encuentra una falla correcta.")]
    [SerializeField] private int maxWrongClicks = 5;

    [Header("Límite de Pistas")]
    [SerializeField] private int maxHints = 3;

    [Header("Audio")]
    [SerializeField] private AudioSource goodTick;
    [SerializeField] private AudioSource badTick;
    [Tooltip("Segundos que espera después de reproducir el sonido antes de abrir el panel.")]
    [SerializeField] private float soundToInfoPanelDelay = 0.4f;

    private int foundCount = 0;
    private int wrongClickCount = 0;
    private int hintsUsed = 0;
    private float currentTime;
    private bool gameRunning = false;

    // ─── ILevelScorer ─────────────────────────────────────────────────────────

    public int LevelIndex => levelIndex;

    public float CalculateScore()
    {
        if (TotalPoints == 0) return 0f;
        return ((float)foundCount / TotalPoints) * 10f;
    }

    // ─── Propiedades ──────────────────────────────────────────────────────────

    public int TotalPoints => allClickPoints != null ? allClickPoints.Count : 0;
    public int FoundCount => foundCount;
    public float RemainingTime => Mathf.Max(currentTime, 0f);
    public int WrongClickCount => wrongClickCount;
    public int MaxWrongClicks => maxWrongClicks;
    public int HintsUsed => hintsUsed;
    public int MaxHints => maxHints;

    private UIManager GetUI() => UIManager.Instance;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Asegurar que el sonido de acierto no se corte con Time.timeScale = 0
        if (goodTick != null) goodTick.ignoreListenerPause = true;
        if (badTick != null) badTick.ignoreListenerPause = true;
    }
    private void Start() => StartGame();

    private void Update()
    {
        if (!gameRunning || !useTimer || GetUI().IsPaused) return;

        currentTime -= Time.deltaTime;
        GetUI().UpdateTimerDisplay(currentTime, timeLimit);

        if (currentTime <= 0f) LoseGame();

        if (Input.GetKeyDown(KeyCode.F1)) LoseGame();
        if (Input.GetKeyDown(KeyCode.F12)) DebugWin();
    }

    private void DebugWin()
    {
        gameRunning = false;
        const float debugScore = 10f;
        SubmitGrade(debugScore);
        Debug.Log("[DEBUG] Victoria forzada con nota 10.");
        GetUI().ShowWin(debugScore, foundCount, TotalPoints);
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    public void StartGame()
    {
        foundCount = 0;
        wrongClickCount = 0;
        hintsUsed = 0;
        currentTime = gameDuration;
        gameRunning = true;

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

        // Reproducir sonido y abrir el panel después del delay
        StartCoroutine(PlaySoundThenShowPanel(point));
    }

    public void RegisterWrongClick()
    {
        if (!gameRunning || GetUI().IsPaused) return;
        if (maxWrongClicks <= 0) return;

        if (badTick != null) badTick.Play();

        wrongClickCount++;
        GetUI().UpdateClickDisplay(wrongClickCount, maxWrongClicks);

        if (wrongClickCount >= maxWrongClicks)
            LoseGame();
    }

    public void RegisterPointFound()
    {
        foundCount++;
        GetUI().UpdateProgressDisplay(foundCount, TotalPoints);

        // Reiniciar el contador de clicks incorrectos al acertar
        wrongClickCount = 0;
        GetUI().UpdateClickDisplay(wrongClickCount, maxWrongClicks);

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

    // ─── Audio + Panel ────────────────────────────────────────────────────────

    /// <summary>
    /// Reproduce el sonido de acierto y espera el delay antes de
    /// abrir el panel de info. Usa WaitForSecondsRealtime para que
    /// el audio no se corte cuando Time.timeScale pasa a 0.
    /// </summary>
    private IEnumerator PlaySoundThenShowPanel(ClickPoint point)
    {
        if (goodTick != null) goodTick.Play();

        yield return new WaitForSecondsRealtime(soundToInfoPanelDelay);

        GetUI().ShowInfoPanel(point);
    }

    // ─── Fin de partida ───────────────────────────────────────────────────────

    private void WinGame()
    {
        gameRunning = false;
        float score = CalculateScore();
        Debug.Log($"[GameManager] WinGame! Score calculado: {score}");
        SubmitGrade(score);
        GetUI().ShowWin(score, foundCount, TotalPoints);
    }

    private void LoseGame()
    {
        gameRunning = false;
        float score = CalculateScore();
        Debug.Log($"[GameManager] LoseGame! Score calculado: {score}");
        SubmitGrade(score);
        GetUI().ShowLose(score, foundCount, TotalPoints);
    }

    private void SubmitGrade(float score)
    {
        var gradeService = ServiceLocator.Instance.GetService("GradeService") as GradeService;
        if (gradeService == null)
            Debug.LogError($"[GameManager] No se encontró GradeService para enviar nota del nivel {levelIndex}.");
        gradeService?.SubmitGrade(levelIndex, score);
    }
}