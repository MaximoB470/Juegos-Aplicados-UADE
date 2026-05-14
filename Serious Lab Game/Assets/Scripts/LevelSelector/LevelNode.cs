using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla el estado visual de un nodo individual en el selector de niveles.
/// Estados: Locked (bloqueado) | Current (activo/brillando) | Completed (tachado)
/// </summary>
public class LevelNode : MonoBehaviour
{
    public enum NodeState { Locked, Current, Completed }

    [Header("Datos")]
    public LevelData levelData;
    public int levelIndex;

    [Header("Referencias UI")]
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image nodeBackground;
    [SerializeField] private GameObject completedOverlay;   // sprite de tilde/tachado
    [SerializeField] private GameObject glowEffect;         // partícula o imagen de glow
    [SerializeField] private TMP_Text labelText;

    [Header("Colores de estado")]
    [SerializeField] private Color lockedColor    = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color currentColor   = Color.white;
    [SerializeField] private Color completedColor = new Color(0.6f, 0.9f, 0.6f, 1f);

    private NodeState currentState;

    // Evento que el LevelSelectorManager escucha para cargar la escena
    public System.Action<LevelNode> OnNodeClicked;

    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
        if (levelData != null && iconImage != null && levelData.levelIcon != null)
            iconImage.sprite = levelData.levelIcon;
        if (labelText != null && levelData != null)
            labelText.text = levelData.levelName;
    }

    // ─── API pública ─────────────────────────────────────────────────────────

    public void SetState(NodeState state)
    {
        currentState = state;
        RefreshVisuals();
    }

    // ─── Privados ────────────────────────────────────────────────────────────

    private void RefreshVisuals()
    {
        switch (currentState)
        {
            case NodeState.Locked:
                nodeBackground.color = lockedColor;
                button.interactable  = false;
                SetActive(completedOverlay, false);
                SetActive(glowEffect, false);
                break;

            case NodeState.Current:
                nodeBackground.color = currentColor;
                button.interactable  = true;
                SetActive(completedOverlay, false);
                SetActive(glowEffect, true);
                break;

            case NodeState.Completed:
                nodeBackground.color = completedColor;
                button.interactable  = false;           // ya jugado, no se puede repetir
                SetActive(completedOverlay, true);
                SetActive(glowEffect, false);
                break;
        }
    }

    private void HandleClick()
    {
        if (currentState != NodeState.Current) return;
        OnNodeClicked?.Invoke(this);
    }

    private static void SetActive(GameObject go, bool value)
    {
        if (go != null) go.SetActive(value);
    }
}
