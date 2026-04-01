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

    private int foundCount = 0;
    private float currentTime;
    private bool gameRunning = false;
    public int TotalPoints => allClickPoints != null ? allClickPoints.Count : 0;
    public int FoundCount => foundCount;
    public float RemainingTime => Mathf.Max(currentTime, 0f);

   
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
        if (!gameRunning || !useTimer || UIManager.Instance.IsPaused) return;
        currentTime -= Time.deltaTime;
        UIManager.Instance.UpdateTimerDisplay(currentTime);
        if (currentTime <= 0f) LoseGame();
    }

    public void StartGame()
    {
        foundCount = 0;
        currentTime = gameDuration;
        gameRunning = true;

        foreach (var cp in allClickPoints)
            cp.ResetPoint();

        UIManager.Instance.HideAll();
        UIManager.Instance.ResumeGame();
        UIManager.Instance.UpdateProgressDisplay(foundCount, TotalPoints);
    }

    public void OnPointClicked(ClickPoint point)
    {
        if (!gameRunning || UIManager.Instance.IsPaused) return;
        if (point.IsFound) return;
        UIManager.Instance.ShowInfoPanel(point);
    }

    public void RegisterPointFound()
    {
        foundCount++;
        UIManager.Instance.UpdateProgressDisplay(foundCount, TotalPoints);

        if (foundCount >= TotalPoints)
            Invoke(nameof(WinGame), 0.15f);
    }

    private void WinGame()
    {
        gameRunning = false;
        UIManager.Instance.ShowWin();
    }

    private void LoseGame()
    {
        gameRunning = false;
        UIManager.Instance.ShowLose();
    }
}