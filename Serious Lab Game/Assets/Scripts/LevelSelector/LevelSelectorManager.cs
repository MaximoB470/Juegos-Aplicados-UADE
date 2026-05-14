using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Orquestador de la escena del selector de niveles.
/// - Lee el progreso desde LevelProgressService.
/// - Instancia o configura los LevelNode del mapa.
/// - Instancia los LevelConnector entre nodos.
/// - Escucha el evento OnLevelProgressChanged para refrescarse en caliente
///   (útil si se navega de vuelta al selector sin recargar la escena).
/// </summary>
public class LevelSelectorManager : MonoBehaviour
{
    [Header("Nodos — arrastrá los LevelNode de la escena en orden")]
    [SerializeField] private List<LevelNode> nodes;

    [Header("Conector — prefab con LevelConnector + Image")]
    [SerializeField] private GameObject connectorPrefab;

    [Header("Parent donde se instancian los conectores")]
    [SerializeField] private RectTransform connectorsParent;

    private LevelProgressService progressService;
    private readonly List<LevelConnector> spawnedConnectors = new();

    private void Start()
    {
        progressService = (LevelProgressService)ServiceLocator.Instance
            .GetService("LevelProgressService");

        if (progressService == null)
        {
            Debug.LogError("[LevelSelectorManager] LevelProgressService no encontrado en ServiceLocator.");
            return;
        }

        // Suscribirse para refrescar si el progreso cambia mientras la escena está abierta
        progressService.OnLevelProgressChanged += OnProgressChanged;

        BuildConnectors();
        RefreshNodes(progressService.CurrentLevelIndex);
        RefreshConnectors(progressService.CurrentLevelIndex);
    }

    private void OnDestroy()
    {
        if (progressService != null)
            progressService.OnLevelProgressChanged -= OnProgressChanged;
    }

    // ─── Callbacks ───────────────────────────────────────────────────────────

    private void OnProgressChanged(int newIndex)
    {
        RefreshNodes(newIndex);
        RefreshConnectors(newIndex);
    }

    // ─── Construcción ────────────────────────────────────────────────────────

    /// <summary>Crea los conectores una sola vez al iniciar la escena.</summary>
    private void BuildConnectors()
    {
        if (connectorPrefab == null || connectorsParent == null) return;

        for (int i = 0; i < nodes.Count - 1; i++)
        {
            var go        = Instantiate(connectorPrefab, connectorsParent);
            var connector = go.GetComponent<LevelConnector>();

            if (connector == null) continue;

            spawnedConnectors.Add(connector);
            // Los conectores se posicionan al hacer el primer refresh
        }
    }

    // ─── Refresh ─────────────────────────────────────────────────────────────

    private void RefreshNodes(int currentIndex)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            node.levelIndex = i;

            LevelNode.NodeState state;
            if      (i < currentIndex)  state = LevelNode.NodeState.Completed;
            else if (i == currentIndex) state = LevelNode.NodeState.Current;
            else                        state = LevelNode.NodeState.Locked;

            node.SetState(state);

            // Suscribir click (puede llamarse varias veces, limpiamos primero)
            node.OnNodeClicked -= HandleNodeClicked;
            if (state == LevelNode.NodeState.Current)
                node.OnNodeClicked += HandleNodeClicked;
        }
    }

    private void RefreshConnectors(int currentIndex)
    {
        for (int i = 0; i < spawnedConnectors.Count; i++)
        {
            if (i >= nodes.Count - 1) break;

            bool completed = i < currentIndex;
            spawnedConnectors[i].Connect(
                nodes[i].GetComponent<RectTransform>(),
                nodes[i + 1].GetComponent<RectTransform>(),
                completed
            );
        }
    }

    // ─── Navegación ──────────────────────────────────────────────────────────

    private void HandleNodeClicked(LevelNode node)
    {
        if (node.levelData == null)
        {
            Debug.LogWarning($"[LevelSelectorManager] El nodo {node.name} no tiene LevelData asignado.");
            return;
        }

        SceneManager.LoadScene(node.levelData.sceneName);
    }
}
