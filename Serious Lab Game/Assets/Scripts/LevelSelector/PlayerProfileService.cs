using UnityEngine;


public class PlayerProfileService : MonoBehaviour
{
    private const string SERVICE_KEY = "PlayerProfileService";

    public static PlayerProfileService Instance { get; private set; }

    public string PlayerName { get; private set; } = string.Empty;

    public bool HasName => !string.IsNullOrWhiteSpace(PlayerName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ServiceLocator.Instance.SetService(SERVICE_KEY, this);
    }

    public void SetPlayerName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
            PlayerName = name.Trim();
    }
}