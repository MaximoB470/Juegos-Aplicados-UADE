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
    public string PlayerName { get; private set; } = "Estudiante";

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
    /// Guarda el nombre del jugador. Se llama desde el menú principal.
    /// Si el nombre está vacío usa el valor por defecto.
    /// </summary>
    public void SetPlayerName(string name)
    {
        PlayerName = string.IsNullOrWhiteSpace(name) ? "Estudiante" : name.Trim();
    }
}
