using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dibuja una línea entre dos RectTransform usando una Image estirada.
/// Agregalo como componente en un GameObject hijo del canvas del selector.
/// Se configura automáticamente desde LevelSelectorManager.
/// </summary>
[RequireComponent(typeof(Image))]
public class LevelConnector : MonoBehaviour
{
    [SerializeField] private Color connectorColor         = new Color(0.5f, 0.5f, 0.5f, 0.8f);
    [SerializeField] private Color connectorCompletedColor = new Color(0.4f, 0.85f, 0.4f, 1f);
    [SerializeField] private float lineWidth = 6f;

    private Image lineImage;
    private RectTransform rectTransform;

    private void Awake()
    {
        lineImage     = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Posiciona y rota la línea para conectar dos nodos.
    /// </summary>
    /// <param name="from">Nodo origen.</param>
    /// <param name="to">Nodo destino.</param>
    /// <param name="completed">Si el tramo ya fue recorrido.</param>
    public void Connect(RectTransform from, RectTransform to, bool completed)
    {
        lineImage.color = completed ? connectorCompletedColor : connectorColor;

        Vector2 fromPos = from.anchoredPosition;
        Vector2 toPos   = to.anchoredPosition;

        Vector2 direction = toPos - fromPos;
        float   distance  = direction.magnitude;
        float   angle     = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rectTransform.anchoredPosition = fromPos + direction * 0.5f;
        rectTransform.sizeDelta        = new Vector2(distance, lineWidth);
        rectTransform.localRotation    = Quaternion.Euler(0f, 0f, angle);
    }
}
