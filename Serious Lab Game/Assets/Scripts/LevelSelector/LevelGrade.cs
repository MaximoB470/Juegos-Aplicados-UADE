
[System.Serializable]
public class LevelGrade
{
    public const float PassingScore = 7f;

    public int   levelIndex;
    public float bestScore;
    public bool  isPassed;
    public bool  hasBeenAttempted;

    public void TryUpdateScore(float newScore)
    {
        hasBeenAttempted = true;

        if (newScore > bestScore)
        {
            bestScore = newScore;
            isPassed  = bestScore >= PassingScore;
        }
    }

    public string FormattedScore => hasBeenAttempted ? $"{bestScore:F1}" : "--";
    public string StatusText
    {
        get
        {
            if (!hasBeenAttempted) return "Sin intentar";
            return isPassed ? "Aprobado" : "Desaprobado";
        }
    }
}
