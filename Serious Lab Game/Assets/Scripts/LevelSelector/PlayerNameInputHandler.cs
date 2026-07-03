using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

            nameInputField.characterLimit = MAX_NAME_LENGTH;
            nameInputField.lineType = TMP_InputField.LineType.SingleLine;

            var placeholder = nameInputField.placeholder.GetComponent<TMP_Text>();
            if (placeholder != null)
                placeholder.text = placeholderText;

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


    private void OnNameChanged(string value)
    {
        bool hasName = !string.IsNullOrWhiteSpace(value);
        SetButtonsInteractable(hasName);

        if (hasName)
            profileService?.SetPlayerName(value);
    }

    public void ConfirmName()
    {
        if (nameInputField != null && !string.IsNullOrWhiteSpace(nameInputField.text))
            profileService?.SetPlayerName(nameInputField.text);
    }

    private void SetButtonsInteractable(bool value)
    {
        if (startButton != null)
            startButton.interactable = value;
        if (levelsButton != null)
            levelsButton.interactable = value;
    }
}