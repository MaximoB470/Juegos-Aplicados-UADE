using System;
using System.Collections.Generic;
using UnityEngine;

public class GradeService : MonoBehaviour
{
    private const string SERVICE_KEY = "GradeService";

    [Header("Configuración")]
    [Tooltip("Total de niveles del juego. Actualizar si se agregan niveles.")]
    [SerializeField] private int totalLevels = 3;

    private readonly Dictionary<int, LevelGrade> grades = new();

    // ─── Eventos ─────────────────────────────────────────────────────────────

    public event Action<int, LevelGrade> OnGradeSubmitted;

    public event Action OnAllLevelsPassed;

    private static GradeService _instance;

    // ─── Lifecycle ───────────────────────────────────────────────────────────

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        ServiceLocator.Instance.SetService(SERVICE_KEY, this);
    }

    private void OnDestroy()
    {
        OnGradeSubmitted  = null;
        OnAllLevelsPassed = null;
    }

    public void SubmitGrade(int levelIndex, float score)
    {
        Debug.Log($"[GradeService] SubmitGrade recibido: Nivel {levelIndex} | Score: {score}");

        if (!grades.ContainsKey(levelIndex))
        {
            grades[levelIndex] = new LevelGrade { levelIndex = levelIndex };
            Debug.Log($"[GradeService] Creada nueva entrada LevelGrade para Nivel {levelIndex}.");
        }

        float roundedScore = Mathf.Round(score * 10f) / 10f; 
        grades[levelIndex].TryUpdateScore(roundedScore);

        Debug.Log($"[GradeService] TryUpdateScore ejecutado. Mejor nota actual: {grades[levelIndex].bestScore}, ¿Aprobado?: {grades[levelIndex].isPassed}");

        OnGradeSubmitted?.Invoke(levelIndex, grades[levelIndex]);

        if (grades[levelIndex].isPassed)
        {
            var progress = ServiceLocator.Instance.GetService("LevelProgressService")
                           as LevelProgressService;
            
            if (progress != null)
            {
                Debug.Log($"[GradeService] Intentando desbloquear el nivel {levelIndex + 1} a través de LevelProgressService.");
                progress.UnlockLevel(levelIndex + 1);
            }
            else
            {
                Debug.LogWarning("[GradeService] No se encontró LevelProgressService para desbloquear el siguiente nivel.");
            }
        }

        if (AllLevelsPassed())
            OnAllLevelsPassed?.Invoke();
    }

    public LevelGrade GetGrade(int levelIndex)
    {
        if (grades.TryGetValue(levelIndex, out var grade))
            return grade;

        return new LevelGrade { levelIndex = levelIndex }; 
    }

    public bool AllLevelsPassed()
    {
        for (int i = 0; i < totalLevels; i++)
        {
            if (!GetGrade(i).isPassed) return false;
        }
        return true;
    }
}
