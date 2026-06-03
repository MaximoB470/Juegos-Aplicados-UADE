using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Maneja el input de nombre del jugador en el menú principal.
/// Al confirmar, guarda el nombre en PlayerProfileService y habilita
/// el botón de inicio.
/// </summary>
public class PlayerNameInputHandler : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button         startButton;
    [Tooltip("Texto placeholder del input.")]
    [SerializeField] private string         placeholderText = "Ingresá tu nombre...";

    private void Start()
    {
        if (nameInputField != null)
        {
            nameInputField.onValueChanged.AddListener(OnNameChanged);
            nameInputField.placeholder.GetComponent<TMP_Text>().text = placeholderText;
        }

        // El botón de inicio empieza deshabilitado hasta que haya un nombre
        SetStartButtonInteractable(false);
    }

    private void OnNameChanged(string value)
    {
        SetStartButtonInteractable(!string.IsNullOrWhiteSpace(value));
    }

    /// <summary>
    /// Llamado por el botón "Confirmar" o al presionar Enter.
    /// Guarda el nombre y habilita el inicio del juego.
    /// </summary>
    public void ConfirmName()
    {
        if (nameInputField == null) return;

        var profile = ServiceLocator.Instance.GetService("PlayerProfileService")
                      as PlayerProfileService;
        profile?.SetPlayerName(nameInputField.text);
    }

    private void SetStartButtonInteractable(bool value)
    {
        if (startButton != null)
            startButton.interactable = value;
    }
}
