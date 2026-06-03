/// <summary>
/// Datos de calificación de un nivel específico.
/// Clase de datos pura — sin dependencias de Unity.
/// </summary>
[System.Serializable]
public class LevelGrade
{
    public const float PassingScore = 7f;

    public int   levelIndex;
    public float bestScore;
    public bool  isPassed;
    public bool  hasBeenAttempted;

    /// <summary>
    /// Actualiza la nota si la nueva es mayor.
    /// Siempre conserva el mejor resultado histórico.
    /// </summary>
    public void TryUpdateScore(float newScore)
    {
        hasBeenAttempted = true;

        if (newScore > bestScore)
        {
            bestScore = newScore;
            isPassed  = bestScore >= PassingScore;
        }
    }

    /// <summary>Nota formateada con un decimal para mostrar en UI.</summary>
    public string FormattedScore => hasBeenAttempted ? $"{bestScore:F1}" : "--";

    /// <summary>Texto de estado para mostrar en UI.</summary>
    public string StatusText
    {
        get
        {
            if (!hasBeenAttempted) return "Sin intentar";
            return isPassed ? "Aprobado" : "Desaprobado";
        }
    }
}
