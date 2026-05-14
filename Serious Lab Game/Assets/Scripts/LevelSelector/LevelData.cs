using UnityEngine;

/// <summary>
/// ScriptableObject que define los datos estáticos de un nivel.
/// Creá uno por nivel: clic derecho → Create → LevelSystem → LevelData
/// </summary>
[CreateAssetMenu(fileName = "LevelData", menuName = "LevelSystem/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Identificación")]
    public string levelName = "Nivel 1";
    public string sceneName = "Level_01";

    [Header("Visual en el selector")]
    [Tooltip("Ícono o ilustración que representa el nivel en el mapa.")]
    public Sprite levelIcon;

    [Tooltip("Descripción corta que aparece en el tooltip del nodo.")]
    [TextArea(2, 4)]
    public string description = "Descripción del nivel.";
}
