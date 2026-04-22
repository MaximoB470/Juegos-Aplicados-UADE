using UnityEngine;

[CreateAssetMenu(menuName = "LabSafe/EPP/EPP Option", fileName = "NewEPPOption")]
public class EPPOptionSO : ScriptableObject
{
    [Tooltip("Texto que verá el jugador en el slider (ej: 'Guardapolvo abrochado')")]
    public string optionLabel;

    [Tooltip("Imagen que representa visualmente esta opción")]
    public Sprite optionIcon;

    [Tooltip("¿Es esta la opción correcta para la categoría en este escenario?")]
    public bool isCorrect;
}
