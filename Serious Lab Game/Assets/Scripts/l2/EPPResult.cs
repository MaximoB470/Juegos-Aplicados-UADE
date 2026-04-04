using System.Collections.Generic;

/// <summary>
/// Clase de datos (no MonoBehaviour) que encapsula el resultado de evaluar
/// la selección del jugador en un escenario EPP.
/// </summary>
public class EPPResult
{
    /// <summary>¿Eligió correctamente la protección de Cabeza?</summary>
    public bool headCorrect;

    /// <summary>¿Eligió correctamente la protección de Cuerpo?</summary>
    public bool bodyCorrect;

    /// <summary>¿Eligió correctamente la protección de Manos?</summary>
    public bool handsCorrect;

    /// <summary>¿Eligió correctamente la protección de Pies?</summary>
    public bool feetCorrect;

    /// <summary>True solo si las cuatro categorías son correctas.</summary>
    public bool allCorrect => headCorrect && bodyCorrect && handsCorrect && feetCorrect;

    /// <summary>
    /// Labels de las opciones INCORRECTAS que eligió el jugador
    /// (una por categoría que falló).
    /// </summary>
    public List<string> incorrectLabels = new List<string>();

    /// <summary>
    /// Labels de las opciones CORRECTAS para cada categoría que el jugador falló
    /// (permiten mostrar "lo correcto era X").
    /// </summary>
    public List<string> correctLabels = new List<string>();

    /// <summary>
    /// Nombres de las categorías falladas, en orden, para armar el desglose de errores.
    /// Paralelo a incorrectLabels y correctLabels.
    /// </summary>
    public List<string> incorrectCategoryNames = new List<string>();

    /// <summary>
    /// Feedback del escenario (correcto o incorrecto según allCorrect)
    /// obtenido directamente del EPPScenarioSO.
    /// </summary>
    public string scenarioFeedback;
}
