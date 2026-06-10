using UnityEngine;

/// <summary>
/// Servicio singleton que almacena el perfil del jugador.
/// El nombre se usa en el diploma y en el selector de niveles.
/// Registrado en ServiceLocator con la clave "PlayerProfileService".
/// </summary>
public class PlayerProfileService : MonoBehaviour
{
    private const string SERVICE_KEY = "PlayerProfileService";

    public static PlayerProfileService Instance { get; private set; }

    /// <summary>Nombre ingresado por el jugador en el menú principal.</summary>
    public string PlayerName { get; private set; } = string.Empty;

    /// <summary>True si el jugador ya confirmó un nombre al menos una vez.</summary>
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

    // ─── API pública ─────────────────────────────────────────────────────────

    /// <summary>
    /// Guarda el nombre del jugador solo si el nuevo valor no está vacío.
    /// Un nombre en blanco es ignorado — el nombre anterior se conserva.
    /// </summary>
    public void SetPlayerName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
            PlayerName = name.Trim();
    }
}