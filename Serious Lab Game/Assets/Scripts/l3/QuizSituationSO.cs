using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Define una situación/pregunta del quiz.
/// Creá uno por pregunta: clic derecho → Create → Quiz → QuizSituation
/// </summary>
[CreateAssetMenu(fileName = "QuizSituation", menuName = "Quiz/QuizSituation")]
public class QuizSituationSO : ScriptableObject
{
    [Header("Pregunta")]
    [TextArea(2, 5)]
    public string situationText = "¿Qué harías si...?";

    [Header("Opciones — asigná exactamente 3")]
    public List<QuizOptionSO> options;
}
