using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Orquestador del selector de niveles.
/// Los nodos y conectores se asignan manualmente en el Inspector —
/// no se instancia nada en runtime.
/// </summary>
public class LevelSelectorManager : MonoBehaviour
{
    [Header("Nodos — en orden de nivel (base 0)")]
    [SerializeField] private List<LevelNode> nodes;

    [Header("Conectores — en orden entre nodos")]
    [Tooltip("Conector 0 = línea entre nodo 0 y nodo 1, etc.")]
    [SerializeField] private List<LevelConnector> connectors;

    private LevelProgressService progressService;
    private GradeService gradeService;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Start()
    {
        progressService = ServiceLocator.Instance.GetService("LevelProgressService") as LevelProgressService;
        gradeService = ServiceLocator.Instance.GetService("GradeService") as GradeService;

        if (progressService == null)
            Debug.LogError("[LevelSelectorManager] LevelProgressService no encontrado.");

        // ✅ Suscripción usando métodos nombrados para evitar memory leaks
        if (progressService != null)
            progressService.OnLevelProgressChanged += OnProgressChanged;

        if (gradeService != null)
            gradeService.OnGradeSubmitted += OnGradeSubmitted;

        Refresh();
    }

    private void OnDestroy()
    {
        // ✅ Ahora la desuscripción sí funciona correctamente porque la referencia es la misma
        if (progressService != null)
            progressService.OnLevelProgressChanged -= OnProgressChanged;

        if (gradeService != null)
            gradeService.OnGradeSubmitted -= OnGradeSubmitted;
    }

    // ─── Handlers de Eventos ──────────────────────────────────────────────────

    private void OnProgressChanged(int _)
    {
        Refresh();
    }

    private void OnGradeSubmitted(int _, LevelGrade __)
    {
        Refresh();
    }

    // ─── Refresh ──────────────────────────────────────────────────────────────

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

            // Evitamos doble suscripción limpiando antes de asignar
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
            // El conector i conecta el nodo i con el nodo i+1
            // Se considera completado si el nodo i ya fue superado
            bool completed = i < currentIndex;
            connectors[i].Refresh(completed);
        }
    }

    // ─── Lógica de estado ─────────────────────────────────────────────────────

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