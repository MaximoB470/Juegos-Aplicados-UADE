using UnityEngine;

public class LevelProgressService : MonoBehaviour
{
    public static LevelProgressService Instance { get; private set; }

    public int CurrentLevelIndex { get; private set; }

    public System.Action<int> OnLevelProgressChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ServiceLocator.Instance.SetService("LevelProgressService", this);

        CurrentLevelIndex = 0;
    }

    public void UnlockLevel(int index)
    {
        if (index <= CurrentLevelIndex) return;

        CurrentLevelIndex = index;
        OnLevelProgressChanged?.Invoke(CurrentLevelIndex);
    }

    public void ResetProgress()
    {
        CurrentLevelIndex = 0;
        OnLevelProgressChanged?.Invoke(CurrentLevelIndex);
    }
}
