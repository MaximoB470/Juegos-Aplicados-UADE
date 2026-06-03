using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Orquestador de la escena del selector de niveles.
/// Consulta LevelProgressService (desbloqueo) y GradeService (notas)
/// para determinar el estado visual de cada nodo.
/// </summary>
public class LevelSelectorManager : MonoBehaviour
{
    [Header("Nodos — en orden de nivel")]
    [SerializeField] private List<LevelNode> nodes;

    [Header("Conector — prefab con LevelConnector + Image")]
    [SerializeField] private GameObject   connectorPrefab;
    [SerializeField] private RectTransform connectorsParent;

    private LevelProgressService           progressService;
    private GradeService                   gradeService;
    private readonly List<LevelConnector>  spawnedConnectors = new();

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Start()
    {
        progressService = ServiceLocator.Instance.GetService("LevelProgressService") as LevelProgressService;
        gradeService    = ServiceLocator.Instance.GetService("GradeService")          as GradeService;

        if (progressService == null)
        {
            Debug.LogError("[LevelSelectorManager] LevelProgressService no encontrado.");
            // No hacemos return para que al menos las notas se puedan leer si falta el progreso
        }
        else
        {
            Debug.Log("[LevelSelectorManager] LevelProgressService encontrado correctamente.");
        }

        progressService.OnLevelProgressChanged += OnProgressChanged;

        if (gradeService != null)
        {
            Debug.Log("[LevelSelectorManager] GradeService encontrado correctamente.");
            gradeService.OnGradeSubmitted += OnGradeSubmitted;
        }
        else
        {
            Debug.LogWarning("[LevelSelectorManager] GradeService no encontrado. Los nodos no reflejarán las notas previas.");
        }

        BuildConnectors();
        Refresh();
    }

    private void OnDestroy()
    {
        if (progressService != null)
            progressService.OnLevelProgressChanged -= OnProgressChanged;

        if (gradeService != null)
            gradeService.OnGradeSubmitted -= OnGradeSubmitted;
    }

    // ─── Callbacks ────────────────────────────────────────────────────────────

    private void OnProgressChanged(int _) => Refresh();
    private void OnGradeSubmitted(int _, LevelGrade __) => Refresh();

    // ─── Construcción ─────────────────────────────────────────────────────────

    private void BuildConnectors()
    {
        if (connectorPrefab == null || connectorsParent == null) return;

        for (int i = 0; i < nodes.Count - 1; i++)
        {
            var go        = Instantiate(connectorPrefab, connectorsParent);
            var connector = go.GetComponent<LevelConnector>();
            if (connector != null) spawnedConnectors.Add(connector);
        }
    }

    // ─── Refresh ──────────────────────────────────────────────────────────────

    private void Refresh()
    {
        int currentIndex = progressService != null ? progressService.CurrentLevelIndex : 0;
        Debug.Log($"[LevelSelectorManager] Refresh() - Nivel desbloqueado actual (currentIndex): {currentIndex}");

        for (int i = 0; i < nodes.Count; i++)
        {
            var node  = nodes[i];
            node.levelIndex = i;

            LevelGrade grade = gradeService?.GetGrade(i);
            
            if (grade != null)
                Debug.Log($"[LevelSelectorManager] Nodo {i}: Grade.bestScore={grade.bestScore}, hasAttempted={grade.hasBeenAttempted}, isPassed={grade.isPassed}");
            else
                Debug.LogWarning($"[LevelSelectorManager] Nodo {i}: GetGrade devolvió null.");

            LevelNode.NodeState state = ResolveState(i, currentIndex, grade);
            Debug.Log($"[LevelSelectorManager] Nodo {i} resuelto como estado: {state}");

            node.SetState(state, grade);

            node.OnNodeClicked -= HandleNodeClicked;
            if (state != LevelNode.NodeState.Locked)
                node.OnNodeClicked += HandleNodeClicked;
        }

        RefreshConnectors(currentIndex);
    }

    /// <summary>
    /// Determina el estado del nodo i:
    ///   i > unlocked        → Locked
    ///   i == unlocked       → Available o Failed (si tuvo intento fallido)
    ///   i menor a unlocked  → Passed (necesariamente aprobó para llegar acá)
    /// </summary>
    private LevelNode.NodeState ResolveState(int index, int currentUnlockedIndex, LevelGrade grade)
    {
        if (index > currentUnlockedIndex)
            return LevelNode.NodeState.Locked;

        if (index < currentUnlockedIndex)
            return LevelNode.NodeState.Passed;

        // index == currentUnlockedIndex
        if (grade != null && grade.hasBeenAttempted && !grade.isPassed)
            return LevelNode.NodeState.Failed;

        return LevelNode.NodeState.Available;
    }

    private void RefreshConnectors(int currentIndex)
    {
        for (int i = 0; i < spawnedConnectors.Count; i++)
        {
            if (i >= nodes.Count - 1) break;

            var fromRect = nodes[i].GetComponent<RectTransform>();
            var toRect   = nodes[i + 1].GetComponent<RectTransform>();

            if (fromRect == null || toRect == null) continue;

            bool completed = i < currentIndex;
            spawnedConnectors[i].Connect(fromRect, toRect, completed);
        }
    }

    // ─── Navegación ───────────────────────────────────────────────────────────

    private void HandleNodeClicked(LevelNode node)
    {
        if (node.levelData == null)
        {
            Debug.LogWarning($"[LevelSelectorManager] Nodo {node.name} sin LevelData.");
            return;
        }

        SceneManager.LoadScene(node.levelData.sceneName);
    }
}
