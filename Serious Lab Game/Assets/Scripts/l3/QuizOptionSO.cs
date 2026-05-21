using UnityEngine;

/// <summary>
/// Define una opción de respuesta dentro de una situación del quiz.
/// Creá uno por opción: clic derecho → Create → Quiz → QuizOption
/// </summary>
[CreateAssetMenu(fileName = "QuizOption", menuName = "Quiz/QuizOption")]
public class QuizOptionSO : ScriptableObject
{
    [Header("Texto mostrado en el botón")]
    public string optionText = "Opción A";

    [Header("¿Es la respuesta correcta?")]
    public bool isCorrect = false;

    [Header("Feedback en el panel de resultado (si es incorrecta)")]
    [TextArea(2, 5)]
    [Tooltip("Explicación de por qué esta opción es incorrecta. Se ignora si isCorrect = true.")]
    public string incorrectFeedback = "Esta opción es incorrecta porque...";

    [Header("Feedback en el panel de resultado (si es correcta)")]
    [TextArea(2, 5)]
    public string correctFeedback = "¡Correcto! Esta es la acción adecuada porque...";
}
