using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMouseMove : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Edge Settings")]
    [SerializeField] private float edgeSize = 50f; 

    [Header("Bounds")]
    [SerializeField] private SpriteRenderer backgroundSprite;

    private Camera cam;

    private Vector2 minBounds;
    private Vector2 maxBounds;

    private void Start()
    {
        cam = Camera.main;

        Bounds bounds = backgroundSprite.bounds;
        minBounds = bounds.min;
        maxBounds = bounds.max;
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        Vector3 move = Vector3.zero;

        if (mousePos.x <= edgeSize)
            move.x = -1;
        else if (mousePos.x >= Screen.width - edgeSize)
            move.x = 1;

        if (mousePos.y <= edgeSize)
            move.y = -1;
        else if (mousePos.y >= Screen.height - edgeSize)
            move.y = 1;

        transform.position += move * moveSpeed * Time.deltaTime;

        ClampPosition();
    }

    private void ClampPosition()
    {
        Vector3 pos = transform.position;

        float camHeight = cam.orthographicSize;
        float camWidth = cam.aspect * camHeight;

        pos.x = Mathf.Clamp(pos.x, minBounds.x + camWidth, maxBounds.x - camWidth);
        pos.y = Mathf.Clamp(pos.y, minBounds.y + camHeight, maxBounds.y - camHeight);

        transform.position = pos;
    }
}