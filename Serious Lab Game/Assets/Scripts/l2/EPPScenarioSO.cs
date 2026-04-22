using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "LabSafe/EPP/EPP Scenario", fileName = "NewEPPScenario")]
public class EPPScenarioSO : ScriptableObject
{
    [Tooltip("Título corto del escenario (ej: 'Vas a encender el mechero')")]
    public string scenarioTitle;

    [Tooltip("Descripción de la situación en segunda persona, tono informal para adolescentes")]
    [TextArea(3, 6)]
    public string scenarioContext;

    [Header("Opciones por categoría")]
    [Tooltip("Opciones para la categoría Cabeza")]
    public List<EPPOptionSO> headOptions;

    [Tooltip("Opciones para la categoría Cuerpo")]
    public List<EPPOptionSO> bodyOptions;

    [Tooltip("Opciones para la categoría Manos")]
    public List<EPPOptionSO> handsOptions;

    [Tooltip("Opciones para la categoría Pies")]
    public List<EPPOptionSO> feetOptions;

    [Header("Feedback")]
    [Tooltip("Mensaje cuando el jugador eligió todo correctamente")]
    [TextArea(2, 4)]
    public string feedbackCorrect;

    [Tooltip("Mensaje base cuando hay errores (se complementa dinámicamente con los errores específicos)")]
    [TextArea(2, 4)]
    public string feedbackIncorrect;
}
