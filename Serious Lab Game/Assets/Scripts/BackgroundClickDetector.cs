using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class BackgroundClickDetector : MonoBehaviour
{
    [Header("Feedback de Click Incorrecto")]
    [Tooltip("RectTransform de la imagen de feedback (ej: ícono de X). Debe estar dentro del Canvas y arrancar desactivada en la escena.")]
    [SerializeField] private RectTransform wrongClickFeedback;

    [Tooltip("Canvas que contiene la imagen de feedback. Necesario para convertir la posición del mouse a coordenadas del Canvas.")]
    [SerializeField] private Canvas canvas;

    [Header("Animación")]
    [SerializeField] private float growTime = 0.15f;
    [SerializeField] private float holdTime = 0.2f;
    [SerializeField] private float fadeTime = 0.25f;
    [SerializeField] private float maxScale = 1.3f;

    private CanvasGroup feedbackCanvasGroup;
    private Coroutine feedbackCoroutine;

    private void Awake()
    {
        if (wrongClickFeedback != null)
        {
            feedbackCanvasGroup = wrongClickFeedback.GetComponent<CanvasGroup>();
            if (feedbackCanvasGroup == null)
                feedbackCanvasGroup = wrongClickFeedback.gameObject.AddComponent<CanvasGroup>();

            wrongClickFeedback.gameObject.SetActive(false);
        }
    }

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
        {
            GameManager.Instance.RegisterWrongClick();
            ShowWrongClickFeedback(Input.mousePosition);
        }
    }

    // ─────────────────────────────────────────────
    // Feedback visual de click incorrecto
    // ─────────────────────────────────────────────

    private void ShowWrongClickFeedback(Vector2 screenPosition)
    {
        if (wrongClickFeedback == null || canvas == null) return;

        RectTransform canvasRect = canvas.transform as RectTransform;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint);

        wrongClickFeedback.localPosition = localPoint;

        // Si ya hay una animación en curso, la cortamos para reiniciarla en la nueva posición
        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        wrongClickFeedback.gameObject.SetActive(true);
        feedbackCoroutine = StartCoroutine(AnimateWrongClickFeedback());
    }

    private IEnumerator AnimateWrongClickFeedback()
    {
        wrongClickFeedback.localScale = Vector3.zero;
        feedbackCanvasGroup.alpha = 1f;

        // Aparece y se agranda
        float t = 0f;
        while (t < growTime)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / growTime);
            wrongClickFeedback.localScale = Vector3.one * Mathf.Lerp(0f, maxScale, p);
            yield return null;
        }
        wrongClickFeedback.localScale = Vector3.one * maxScale;

        // Se mantiene visible un momento
        yield return new WaitForSecondsRealtime(holdTime);

        // Desaparece (fade out)
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeTime);
            feedbackCanvasGroup.alpha = Mathf.Lerp(1f, 0f, p);
            yield return null;
        }

        feedbackCanvasGroup.alpha = 0f;
        wrongClickFeedback.gameObject.SetActive(false);
        feedbackCoroutine = null;
    }
}