using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Maneja el input de nombre del jugador en el menú principal.
/// Si el jugador ya tiene un nombre guardado, pre-rellena el campo
/// y habilita el botón de inicio directamente.
/// Un nombre vacío nunca sobreescribe uno ya guardado.
/// </summary>
public class PlayerNameInputHandler : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button startButton;
    [SerializeField] private Button levelsButton;
    [SerializeField] private string placeholderText = "Ingresá tu nombre...";

    private const int MAX_NAME_LENGTH = 15;

    private PlayerProfileService profileService;

    private void Start()
    {
        profileService = ServiceLocator.Instance.GetService("PlayerProfileService")
                         as PlayerProfileService;

        if (nameInputField != null)
        {
            // Límite de caracteres y sin saltos de línea
            nameInputField.characterLimit = MAX_NAME_LENGTH;
            nameInputField.lineType = TMP_InputField.LineType.SingleLine;

            var placeholder = nameInputField.placeholder.GetComponent<TMP_Text>();
            if (placeholder != null)
                placeholder.text = placeholderText;

            // Si ya hay un nombre guardado, pre-rellenar el campo
            if (profileService != null && profileService.HasName)
            {
                nameInputField.SetTextWithoutNotify(profileService.PlayerName);
                SetButtonsInteractable(true);
            }
            else
            {
                SetButtonsInteractable(false);
            }

            nameInputField.onValueChanged.AddListener(OnNameChanged);
        }
        else
        {
            SetButtonsInteractable(false);
        }
    }

    // ─── Callbacks ────────────────────────────────────────────────────────────

    private void OnNameChanged(string value)
    {
        bool hasName = !string.IsNullOrWhiteSpace(value);
        SetButtonsInteractable(hasName);

        // Solo guarda si el nuevo valor no está vacío —
        // si borra el campo, el nombre anterior se conserva en el servicio
        if (hasName)
            profileService?.SetPlayerName(value);
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Puede llamarse desde el botón Start para garantizar
    /// que el nombre esté guardado antes de cambiar de escena.
    /// </summary>
    public void ConfirmName()
    {
        if (nameInputField != null && !string.IsNullOrWhiteSpace(nameInputField.text))
            profileService?.SetPlayerName(nameInputField.text);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private void SetButtonsInteractable(bool value)
    {
        if (startButton != null)
            startButton.interactable = value;
        if (levelsButton != null)
            levelsButton.interactable = value;
    }
}