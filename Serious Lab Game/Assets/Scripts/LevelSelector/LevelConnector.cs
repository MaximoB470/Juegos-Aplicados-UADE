using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Conector visual entre dos nodos del selector de niveles.
/// Se coloca manualmente en la escena.
/// Genera segmentos hijos como Images para el efecto punteado —
/// sin depender de Image.Type.Tiled ni sprites externos.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class LevelConnector : MonoBehaviour
{
    [Header("Colores")]
    [SerializeField] private Color completedColor = new Color(0.35f, 0.85f, 0.35f, 1f);
    [SerializeField] private Color pendingColor = new Color(0.65f, 0.65f, 0.65f, 0.8f);

    [Header("Configuración de segmentos")]
    [Tooltip("Largo de cada segmento visible (píxeles UI).")]
    [SerializeField] private float dashLength = 10f;
    [Tooltip("Espacio vacío entre segmentos (píxeles UI).")]
    [SerializeField] private float gapLength = 7f;

    private RectTransform rt;
    private readonly List<GameObject> segments = new();

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        rt = GetComponent<RectTransform>();

        // Ocultar la Image base — solo usamos los hijos
        var baseImage = GetComponent<Image>();
        if (baseImage != null) baseImage.color = Color.clear;
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// completed = true  → línea sólida verde.
    /// completed = false → línea punteada gris.
    /// </summary>
    public void Refresh(bool completed)
    {
        ClearSegments();

        float totalLength = rt.rect.width;
        float height = rt.rect.height;
        Color color = completed ? completedColor : pendingColor;

        if (completed)
        {
            // Un solo segmento sólido que cubre todo el largo
            SpawnSegment(0f, totalLength, height, color);
        }
        else
        {
            // Segmentos cortos con espacios entre ellos
            float x = 0f;
            while (x + dashLength <= totalLength)
            {
                SpawnSegment(x, dashLength, height, color);
                x += dashLength + gapLength;
            }
        }
    }

    // ─── Privados ─────────────────────────────────────────────────────────────

    private void SpawnSegment(float xOffset, float width, float height, Color color)
    {
        var go = new GameObject("seg", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);

        var segRt = go.GetComponent<RectTransform>();
        segRt.anchorMin = new Vector2(0f, 0.5f);
        segRt.anchorMax = new Vector2(0f, 0.5f);
        segRt.pivot = new Vector2(0f, 0.5f);
        segRt.sizeDelta = new Vector2(width, height);
        segRt.anchoredPosition = new Vector2(xOffset, 0f);

        go.GetComponent<Image>().color = color;

        segments.Add(go);
    }

    private void ClearSegments()
    {
        foreach (var s in segments)
            if (s != null) Destroy(s);
        segments.Clear();
    }
}