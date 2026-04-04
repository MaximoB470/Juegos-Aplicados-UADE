using UnityEngine;

/// <summary>
/// ScriptableObject que representa una opción dentro de una categoría de EPP.
/// Crear desde: Assets > Create > LabSafe/EPP > EPP Option
/// </summary>
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
