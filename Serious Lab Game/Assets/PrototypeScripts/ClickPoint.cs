using UnityEngine;

public class ClickPoint : MonoBehaviour
{
    private void OnMouseDown()
    {
        GameManager.Instance.OnPointClicked(gameObject);
    }
}