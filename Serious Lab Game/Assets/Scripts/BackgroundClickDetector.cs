using UnityEngine;
using UnityEngine.EventSystems;

public class BackgroundClickDetector : MonoBehaviour
{
    // BackgroundClickDetector.cs
    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (UIManager.Instance.IsPaused) return;

        // Si el click fue sobre un elemento de UI (botón, panel, etc.), lo ignoramos
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Vector2.zero);

        ClickPoint foundPoint = null;
        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent<ClickPoint>(out var cp))
            {
                foundPoint = cp;
                break;
            }
        }

        if (foundPoint != null)
            GameManager.Instance.OnPointClicked(foundPoint);
        else
            GameManager.Instance.RegisterWrongClick();
    }
}