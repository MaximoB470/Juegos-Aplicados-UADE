using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Click Points")]
    [Tooltip("Arrastrá aquí todos los ClickPoint del nivel. El orden no importa.")]
    [SerializeField] private List<ClickPoint> allClickPoints;

    [Header("Timer (opcional)")]
    [SerializeField] private bool useTimer = false;
    [SerializeField] private float gameDuration = 180f;

    [Header("Límite de Clicks")]
    [Tooltip("Cantidad máxima de clicks incorrectos permitidos antes de perder. 0 = sin límite.")]
    [SerializeField] private int maxWrongClicks = 10;

    [Header("Límite de Pistas")]
    [Tooltip("Cantidad máxima de pistas que el jugador puede usar.")]
    [SerializeField] private int maxHints = 3;

    private int foundCount = 0;
    private int wrongClickCount = 0;
    private int hintsUsed = 0;
    private float currentTime;
    private bool gameRunning = false;

    public int TotalPoints => allClickPoints != null ? allClickPoints.Count : 0;
    public int FoundCount => foundCount;
    public float RemainingTime => Mathf.Max(currentTime, 0f);
    public int WrongClickCount => wrongClickCount;
    public int MaxWrongClicks => maxWrongClicks;
    public int HintsUsed => hintsUsed;
    public int MaxHints => maxHints;

    private UIManager GetUI() => UIManager.Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (!gameRunning || !useTimer || GetUI().IsPaused) return;

        currentTime -= Time.deltaTime;
        GetUI().UpdateTimerDisplay(currentTime);
        if (currentTime <= 0f) LoseGame();

        if (gameRunning && Input.GetKeyDown(KeyCode.F1))
            LoseGame();
    }

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

    /// <summary>
    /// Llamado desde ClickPoint cuando el jugador hace click sobre él.
    /// Si el punto ya fue encontrado, cuenta como click incorrecto/spam.
    /// </summary>
    public void OnPointClicked(ClickPoint point)
    {
        if (!gameRunning || GetUI().IsPaused) return;
        if (point.IsFound) return;

        GetUI().ShowInfoPanel(point);
    }

    /// <summary>
    /// Llamado cuando el jugador hace click en un área sin ClickPoint válido.
    /// </summary>
    public void RegisterWrongClick()
    {
        if (!gameRunning || GetUI().IsPaused) return;
        if (maxWrongClicks <= 0) return; // sin límite

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
        if (hintsUsed >= maxHints) return; // no quedan pistas

        List<ClickPoint> remainingPoints = new List<ClickPoint>();
        foreach (var cp in allClickPoints)
            if (!cp.IsFound) remainingPoints.Add(cp);

        if (remainingPoints.Count > 0)
        {
            hintsUsed++;
            ClickPoint randomPoint = remainingPoints[Random.Range(0, remainingPoints.Count)];
            GetUI().ShowHintPanel(randomPoint);
            GetUI().UpdateHintButton(hintsUsed, maxHints);
        }
    }

    private void WinGame()
    {
        gameRunning = false;
        GetUI().ShowWin();
    }

    private void LoseGame()
    {
        gameRunning = false;
        GetUI().ShowLose();
    }
}