using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "QuizSituation", menuName = "Quiz/QuizSituation")]
public class QuizSituationSO : ScriptableObject
{
    [Header("Pregunta")]
    [TextArea(2, 5)]
    public string situationText = "¿Qué harías si...?";

    [Header("Opciones — asigná exactamente 3")]
    public List<QuizOptionSO> options;
}
