using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelNode : MonoBehaviour
{
    public enum NodeState { Locked, Available, Passed, Failed }

    [Header("Datos")]
    public LevelData levelData;
    public int       levelIndex;

    [Header("UI — nodo")]
    [SerializeField] private Button   button;
    [SerializeField] private Image    nodeBackground;
    [SerializeField] private Image    iconImage;
    [SerializeField] private TMP_Text labelText;

    [Header("UI — overlays de estado")]
    [SerializeField] private GameObject glowEffect;
    [SerializeField] private GameObject passedOverlay;
    [SerializeField] private GameObject failedOverlay;
    [SerializeField] private GameObject lockedOverlay;

    [Header("UI — nota y estado")]
    [SerializeField] private TMP_Text gradeText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text retryHintText;

    [Header("Colores por estado")]
    [SerializeField] private Color lockedColor    = new Color(0.25f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color passedColor    = new Color(0.55f, 0.90f, 0.55f, 1f);
    [SerializeField] private Color failedColor    = new Color(0.95f, 0.45f, 0.45f, 1f);

    public System.Action<LevelNode> OnNodeClicked;

    private void Awake()
    {
        button.onClick.AddListener(() => OnNodeClicked?.Invoke(this));

        if (levelData == null) return;
        if (iconImage != null && levelData.levelIcon != null) iconImage.sprite = levelData.levelIcon;
        if (labelText != null) labelText.text = levelData.levelName;
    }

    public void SetState(NodeState state, LevelGrade grade = null)
    {
        if (nodeBackground != null)
            nodeBackground.color = state switch
            {
                NodeState.Locked    => lockedColor,
                NodeState.Available => availableColor,
                NodeState.Passed    => passedColor,
                NodeState.Failed    => failedColor,
                _                   => availableColor
            };

        SetActive(glowEffect,    state == NodeState.Available);
        SetActive(passedOverlay, state == NodeState.Passed);
        SetActive(failedOverlay, state == NodeState.Failed);
        SetActive(lockedOverlay, state == NodeState.Locked);

        button.interactable = state != NodeState.Locked;

        RefreshGradeDisplay(state, grade);
    }

    private void RefreshGradeDisplay(NodeState state, LevelGrade grade)
    {
        if (grade != null && grade.hasBeenAttempted)
        {
            if (gradeText  != null) gradeText.text  = grade.FormattedScore;
            if (statusText != null) statusText.text  = grade.StatusText;

            if (retryHintText != null)
            {
                retryHintText.gameObject.SetActive(true);
                retryHintText.text = state == NodeState.Failed
                    ? "Debés aprobar para avanzar"
                    : "Podés reintentar para mejorar tu nota";
            }
        }
        else
        {
            if (gradeText  != null) gradeText.text  = "--";
            if (statusText != null) statusText.text  = state == NodeState.Locked ? "Bloqueado" : "Sin intentar";
            if (retryHintText != null) retryHintText.gameObject.SetActive(false);
        }
    }

    private static void SetActive(GameObject go, bool value)
    {
        if (go != null) go.SetActive(value);
    }
}
