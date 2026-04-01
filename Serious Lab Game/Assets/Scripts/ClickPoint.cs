using UnityEngine;

/// <summary>
/// Colocá este componente en cada objeto de error del nivel.
/// Requiere un CircleCollider2D (o SphereCollider en 3D) configurado como trigger.
/// El sprite/renderer de "foundMarker" se activa cuando el jugador encuentra el error.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ClickPoint : MonoBehaviour
{
    [Header("Contenido del panel")]
    [SerializeField] private string errorTitle = "Título del error";
    [TextArea(3, 6)]
    [SerializeField] private string errorDescription = "Descripción educativa del error.";

    [Header("Marcador visual (found)")]
    [Tooltip("Objeto hijo que se activa al encontrar el punto (ej: tilde verde, ícono de alerta).")]
    [SerializeField] private GameObject foundMarker;

    [Header("Highlight (hover opcional)")]
    [Tooltip("Renderer a colorear mientras el cursor está encima. Dejá vacío si no querés highlight.")]
    [SerializeField] private SpriteRenderer highlightRenderer;
    [SerializeField] private Color highlightColor = new Color(1f, 0.8f, 0f, 0.35f);

    // ── Estado ───────────────────────────────────────────────────────────────
    public bool IsFound { get; private set; }
    public string ErrorTitle => errorTitle;
    public string ErrorDescription => errorDescription;

    private Color originalColor;
    private Collider2D col;

    // ────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        col = GetComponent<Collider2D>();
        if (highlightRenderer != null)
            originalColor = highlightRenderer.color;
    }

    public void ResetPoint()
    {
        IsFound = false;
        col.enabled = true;
        if (foundMarker != null) foundMarker.SetActive(false);
        if (highlightRenderer != null) highlightRenderer.color = originalColor;
    }

    // ── Click ────────────────────────────────────────────────────────────────
    private void OnMouseDown()
    {
        if (IsFound) return;
        GameManager.Instance.OnPointClicked(this);
    }

    // ── Hover highlight (opcional) ───────────────────────────────────────────
    private void OnMouseEnter()
    {
        if (IsFound || highlightRenderer == null) return;
        highlightRenderer.color = highlightColor;
    }

    private void OnMouseExit()
    {
        if (highlightRenderer == null) return;
        highlightRenderer.color = originalColor;
    }

    // ── Marcar como encontrado (llamado por UIManager al cerrar el panel) ────
    public void MarkAsFound()
    {
        IsFound = true;
        col.enabled = false; // desactivar collider para que no sea clickeable de nuevo
        if (foundMarker != null) foundMarker.SetActive(true);
        if (highlightRenderer != null) highlightRenderer.color = originalColor;
    }
}