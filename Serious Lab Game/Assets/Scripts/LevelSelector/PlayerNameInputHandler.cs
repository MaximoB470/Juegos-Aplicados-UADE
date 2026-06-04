using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Maneja el input de nombre del jugador en el menú principal.
/// Guarda el nombre en tiempo real mientras el jugador escribe,
/// sin depender del orden de ejecución del botón de inicio.
/// </summary>
public class PlayerNameInputHandler : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button startButton;
    [SerializeField] private string placeholderText = "Ingresá tu nombre...";

    private PlayerProfileService profileService;

    private void Start()
    {
        profileService = ServiceLocator.Instance.GetService("PlayerProfileService")
                         as PlayerProfileService;

        if (nameInputField != null)
        {
            nameInputField.onValueChanged.AddListener(OnNameChanged);

            var placeholder = nameInputField.placeholder.GetComponent<TMP_Text>();
            if (placeholder != null)
                placeholder.text = placeholderText;
        }

        // Empieza deshabilitado hasta que haya un nombre
        SetStartButtonInteractable(false);
    }

    // ─── Callbacks ────────────────────────────────────────────────────────────

    private void OnNameChanged(string value)
    {
        bool hasName = !string.IsNullOrWhiteSpace(value);
        SetStartButtonInteractable(hasName);

        // Guardar en tiempo real — así no importa si ConfirmName() se llama o no
        profileService?.SetPlayerName(value);
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Puede seguir llamándose desde el botón Start para garantizar
    /// que el nombre esté guardado, pero ya no es el único punto de guardado.
    /// </summary>
    public void ConfirmName()
    {
        if (nameInputField != null)
            profileService?.SetPlayerName(nameInputField.text);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private void SetStartButtonInteractable(bool value)
    {
        if (startButton != null)
            startButton.interactable = value;
    }
}