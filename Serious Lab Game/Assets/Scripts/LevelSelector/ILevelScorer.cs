/// <summary>
/// Contrato que todo GameManager de nivel debe implementar.
/// Garantiza que el sistema de notas puede pedir el score
/// a cualquier nivel sin conocer su implementación interna.
/// </summary>
public interface ILevelScorer
{
    /// <summary>Índice del nivel (base 0). Debe coincidir con el LevelData.</summary>
    int LevelIndex { get; }

    /// <summary>
    /// Calcula la nota del intento actual normalizada entre 0 y 10.
    /// El cálculo usa los datos reales del nivel — nunca valores hardcodeados.
    /// </summary>
    float CalculateScore();
}
