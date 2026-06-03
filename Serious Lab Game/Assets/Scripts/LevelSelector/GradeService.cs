using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Servicio singleton que centraliza el sistema de calificaciones.
/// 
/// Responsabilidades:
/// - Almacenar la mejor nota de cada nivel en memoria.
/// - Determinar si un nivel está aprobado (nota >= 7).
/// - Ordenar el desbloqueo del siguiente nivel a LevelProgressService.
/// - Notificar cuando todos los niveles están aprobados (diploma).
/// 
/// Registrado en ServiceLocator con la clave "GradeService".
/// </summary>
public class GradeService : MonoBehaviour
{
    private const string SERVICE_KEY = "GradeService";

    [Header("Configuración")]
    [Tooltip("Total de niveles del juego. Actualizar si se agregan niveles.")]
    [SerializeField] private int totalLevels = 3;

    private readonly Dictionary<int, LevelGrade> grades = new();

    // ─── Eventos ─────────────────────────────────────────────────────────────

    /// <summary>Se dispara cuando se registra o actualiza una nota.</summary>
    public event Action<int, LevelGrade> OnGradeSubmitted;

    /// <summary>Se dispara cuando todos los niveles están aprobados.</summary>
    public event Action OnAllLevelsPassed;

    // ─── Lifecycle ───────────────────────────────────────────────────────────

    private void Awake()
    {
        ServiceLocator.Instance.SetService(SERVICE_KEY, this);
    }

    private void OnDestroy()
    {
        OnGradeSubmitted  = null;
        OnAllLevelsPassed = null;
    }

    // ─── API pública ─────────────────────────────────────────────────────────

    /// <summary>
    /// Registra la nota de un intento. Si es mejor que la anterior, la reemplaza.
    /// Si es aprobatoria, desbloquea el siguiente nivel.
    /// </summary>
    public void SubmitGrade(int levelIndex, float score)
    {
        if (!grades.ContainsKey(levelIndex))
            grades[levelIndex] = new LevelGrade { levelIndex = levelIndex };

        float roundedScore = Mathf.Round(score * 10f) / 10f; // 1 decimal
        grades[levelIndex].TryUpdateScore(roundedScore);

        OnGradeSubmitted?.Invoke(levelIndex, grades[levelIndex]);

        // Desbloquear el siguiente nivel solo si aprobó
        if (grades[levelIndex].isPassed)
        {
            var progress = ServiceLocator.Instance.GetService("LevelProgressService")
                           as LevelProgressService;
            progress?.UnlockLevel(levelIndex + 1);
        }

        // Verificar condición de diploma
        if (AllLevelsPassed())
            OnAllLevelsPassed?.Invoke();
    }

    /// <summary>
    /// Devuelve la nota de un nivel. Si no fue intentado, devuelve una nota vacía.
    /// </summary>
    public LevelGrade GetGrade(int levelIndex)
    {
        if (grades.TryGetValue(levelIndex, out var grade))
            return grade;

        return new LevelGrade { levelIndex = levelIndex }; // vacía, sin intento
    }

    /// <summary>Devuelve true si todos los niveles tienen nota aprobatoria.</summary>
    public bool AllLevelsPassed()
    {
        for (int i = 0; i < totalLevels; i++)
        {
            if (!GetGrade(i).isPassed) return false;
        }
        return true;
    }
}
