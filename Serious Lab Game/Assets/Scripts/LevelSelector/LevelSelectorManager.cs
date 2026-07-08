using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectorManager : MonoBehaviour
{
    [Header("Nodos — en orden de nivel (base 0)")]
    [SerializeField] private List<LevelNode> nodes;

    [Header("Conectores — en orden entre nodos")]
    [Tooltip("Conector 0 = línea entre nodo 0 y nodo 1, etc.")]
    [SerializeField] private List<LevelConnector> connectors;

    [Header("Panel de informacion")]
    [SerializeField] private GameObject infoPanel;

    private LevelProgressService progressService;
    private GradeService gradeService;
    private TMP_Text levelTitleText;
    private TMP_Text levelDescriptionText;
    private LevelNode selectedNode;

    private void Start()
    {
        progressService = ServiceLocator.Instance.GetService("LevelProgressService") as LevelProgressService;
        gradeService = ServiceLocator.Instance.GetService("GradeService") as GradeService;

        if (progressService == null)
            Debug.LogError("[LevelSelectorManager] LevelProgressService no encontrado.");

        if (progressService != null)
            progressService.OnLevelProgressChanged += OnProgressChanged;

        if (gradeService != null)
            gradeService.OnGradeSubmitted += OnGradeSubmitted;

        CacheInfoPanelReferences();
        CloseInfoPanel();
        Refresh();
    }

    private void OnDestroy()
    {
        if (progressService != null)
            progressService.OnLevelProgressChanged -= OnProgressChanged;

        if (gradeService != null)
            gradeService.OnGradeSubmitted -= OnGradeSubmitted;
    }

    private void OnProgressChanged(int _)
    {
        Refresh();
    }

    private void OnGradeSubmitted(int _, LevelGrade __)
    {
        Refresh();
    }

    private void Refresh()
    {
        int currentIndex = progressService != null ? progressService.CurrentLevelIndex : 0;

        RefreshNodes(currentIndex);
        RefreshConnectors(currentIndex);
    }

    private void RefreshNodes(int currentIndex)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            node.levelIndex = i;

            LevelGrade grade = gradeService?.GetGrade(i);
            LevelNode.NodeState state = ResolveState(i, currentIndex, grade);

            node.SetState(state, grade);

            node.OnNodeClicked -= HandleNodeClicked;
            if (state != LevelNode.NodeState.Locked)
                node.OnNodeClicked += HandleNodeClicked;
        }
    }

    private void RefreshConnectors(int currentIndex)
    {
        for (int i = 0; i < connectors.Count; i++)
        {
            if (connectors[i] == null) continue;
            bool completed = i < currentIndex;
            connectors[i].Refresh(completed);
        }
    }


    private LevelNode.NodeState ResolveState(int index, int currentUnlockedIndex, LevelGrade grade)
    {
        if (index > currentUnlockedIndex)
            return LevelNode.NodeState.Locked;

        if (index < currentUnlockedIndex)
            return LevelNode.NodeState.Passed;

        if (grade != null && grade.hasBeenAttempted && !grade.isPassed)
            return LevelNode.NodeState.Failed;

        return LevelNode.NodeState.Available;
    }


    private void HandleNodeClicked(LevelNode node)
    {
        if (node.levelData == null)
        {
            Debug.LogWarning($"[LevelSelectorManager] Nodo {node.name} sin LevelData.");
            return;
        }

        selectedNode = node;

        if (levelTitleText != null)
            levelTitleText.text = string.IsNullOrWhiteSpace(node.levelData.infoTitle)
                ? node.levelData.levelName
                : node.levelData.infoTitle;

        if (levelDescriptionText != null)
            levelDescriptionText.text = node.levelData.description;

        if (infoPanel != null)
            infoPanel.SetActive(true);
    }

    public void ContinueToSelectedLevel()
    {
        if (selectedNode == null || selectedNode.levelData == null)
        {
            Debug.LogWarning("[LevelSelectorManager] No hay nivel seleccionado para continuar.");
            return;
        }

        SceneManager.LoadScene(selectedNode.levelData.sceneName);
    }

    public void CloseInfoPanel()
    {
        selectedNode = null;

        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    private void CacheInfoPanelReferences()
    {
        if (infoPanel == null)
        {
            Debug.LogWarning("[LevelSelectorManager] InfoPanel no asignado.");
            return;
        }

        foreach (TMP_Text text in infoPanel.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.name == "Title")
                levelTitleText = text;
            else if (text.name == "Description")
                levelDescriptionText = text;
        }

        foreach (Button button in infoPanel.GetComponentsInChildren<Button>(true))
        {
            if (button.name == "ContinueButton")
                button.onClick.AddListener(ContinueToSelectedLevel);
            else if (button.name == "GoBackButton")
                button.onClick.AddListener(CloseInfoPanel);
        }
    }
}
