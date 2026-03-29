using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Click Points")]
    [SerializeField] private List<GameObject> allPoints;
    [SerializeField] private int pointsToActivate = 3;

    [Header("Timer")]
    [SerializeField] private float gameDuration = 180f;

    private List<GameObject> activePoints = new List<GameObject>();

    private float currentTime;
    private bool gameRunning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (!gameRunning || UIManager.Instance.IsPaused) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            LoseGame();
        }
    }

    public void StartGame()
    {
        ResetPoints();

        currentTime = gameDuration;
        gameRunning = true;

        ActivateRandomPoints();

        UIManager.Instance.HideAll();
        UIManager.Instance.ResumeGame();
    }

    private void ResetPoints()
    {
        foreach (var point in allPoints)
        {
            point.SetActive(false);
        }

        activePoints.Clear();
    }

    private void ActivateRandomPoints()
    {
        List<GameObject> shuffled = allPoints.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < pointsToActivate && i < shuffled.Count; i++)
        {
            GameObject point = shuffled[i];
            point.SetActive(true);
            activePoints.Add(point);
        }
    }

    public void OnPointClicked(GameObject point)
    {
        if (!gameRunning || UIManager.Instance.IsPaused) return;
        if (!activePoints.Contains(point)) return;

        point.SetActive(false);
        activePoints.Remove(point);

        UIManager.Instance.ShowInfoPanel();

        if (activePoints.Count == 0)
        {
            Invoke(nameof(WinGame), 0.1f);
        }
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

    public float GetRemainingTime()
    {
        return Mathf.Max(currentTime, 0);
    }
}