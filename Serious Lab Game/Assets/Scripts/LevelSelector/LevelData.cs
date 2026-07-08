using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "LevelSystem/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Identificación")]
    public string levelName = "Nivel 1";
    public string sceneName = "Level_01";
    public string infoTitle = "Titulo del nivel";

    [Header("Visual en el selector")]
    [Tooltip("Ícono o ilustración que representa el nivel en el mapa.")]
    public Sprite levelIcon;

    [Tooltip("Descripción corta que aparece en el tooltip del nodo.")]
    [TextArea(2, 4)]
    public string description = "Descripción del nivel.";
}
