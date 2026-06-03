using UnityEngine;

/// <summary>
/// Servicio singleton que rastrea qué niveles están desbloqueados.
/// El desbloqueo ahora es responsabilidad de GradeService —
/// este servicio solo almacena el índice máximo alcanzado.
/// </summary>
public class LevelProgressService : MonoBehaviour
{
    public static LevelProgressService Instance { get; private set; }

    /// <summary>Índice del nivel desbloqueado más alto (base 0).</summary>
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

    // ─── API pública ─────────────────────────────────────────────────────────

    /// <summary>
    /// Desbloquea el nivel en el índice indicado si es mayor al actual.
    /// Llamado exclusivamente por GradeService cuando se aprueba un nivel.
    /// </summary>
    public void UnlockLevel(int index)
    {
        if (index <= CurrentLevelIndex) return;

        CurrentLevelIndex = index;
        OnLevelProgressChanged?.Invoke(CurrentLevelIndex);
    }

    /// <summary>Resetea el progreso al primer nivel (debug / nuevo juego).</summary>
    public void ResetProgress()
    {
        CurrentLevelIndex = 0;
        OnLevelProgressChanged?.Invoke(CurrentLevelIndex);
    }
}
