using UnityEngine;

/// <summary>
/// Servicio singleton que persiste entre escenas y lleva el registro
/// del nivel actual desbloqueado. El progreso vive en memoria y se
/// resetea automáticamente al detener el juego.
///
/// Uso desde otro script:
///   var progress = (LevelProgressService)ServiceLocator.Instance.GetService("LevelProgressService");
///   progress.CompleteCurrentLevel();
/// </summary>
public class LevelProgressService : MonoBehaviour
{
    public static LevelProgressService Instance { get; private set; }

    /// <summary>Índice del nivel desbloqueado / activo (base 0).</summary>
    public int CurrentLevelIndex { get; private set; }

    // Evento que el LevelSelectorManager escucha para refrescar la UI
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

    // ─── API pública ─────────────────────────────────────────────────────────

    /// <summary>
    /// Marca el nivel actual como completado y desbloquea el siguiente.
    /// Llamar esto al terminar un nivel (desde WinGame o HandleLevelComplete).
    /// </summary>
    public void CompleteCurrentLevel()
    {
        CurrentLevelIndex++;
        OnLevelProgressChanged?.Invoke(CurrentLevelIndex);
    }

    /// <summary>
    /// Fuerza un índice concreto (útil para debug o saltar niveles).
    /// </summary>
    public void SetCurrentLevel(int index)
    {
        CurrentLevelIndex = Mathf.Max(0, index);
        OnLevelProgressChanged?.Invoke(CurrentLevelIndex);
    }

    /// <summary>Resetea el progreso al primer nivel.</summary>
    public void ResetProgress()
    {
        SetCurrentLevel(0);
    }
}