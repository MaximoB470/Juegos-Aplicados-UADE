public interface ILevelScorer
{
    int LevelIndex { get; }

    float CalculateScore();
}
