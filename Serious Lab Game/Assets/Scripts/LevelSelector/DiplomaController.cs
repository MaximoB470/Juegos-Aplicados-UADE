using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiplomaController : MonoBehaviour
{
    [Header("Botón de diploma (en el mapa)")]
    [SerializeField] private Button diplomaButton;

    [Header("Panel del diploma")]
    [SerializeField] private GameObject diplomaPanel;
    [SerializeField] private TMP_Text   playerNameText;
    [Tooltip("Texto fijo que rodea el nombre, ej: 'se otorga a'.")]
    [SerializeField] private string     namePrefix = "Otorgado a: ";
    [SerializeField] private Button     closeButton;

    private GradeService         gradeService;
    private PlayerProfileService profileService;


    private void Start()
    {
        gradeService   = ServiceLocator.Instance.GetService("GradeService")   as GradeService;
        profileService = ServiceLocator.Instance.GetService("PlayerProfileService") as PlayerProfileService;

        if (gradeService != null)
            gradeService.OnAllLevelsPassed += HandleAllLevelsPassed;

        diplomaButton?.onClick.AddListener(OpenDiploma);
        closeButton?.onClick.AddListener(CloseDiploma);

        diplomaPanel?.SetActive(false);

        RefreshDiplomaButton();
    }

    private void OnDestroy()
    {
        if (gradeService != null)
            gradeService.OnAllLevelsPassed -= HandleAllLevelsPassed;
    }

    // ─── Handlers ────────────────────────────────────────────────────────────

    private void HandleAllLevelsPassed()
    {
        SetDiplomaButtonInteractable(true);
    }

    private void OpenDiploma()
    {
        if (playerNameText != null && profileService != null)
            playerNameText.text = namePrefix + profileService.PlayerName;

        diplomaPanel?.SetActive(true);
    }

    private void CloseDiploma()
    {
        diplomaPanel?.SetActive(false);
    }


    private void RefreshDiplomaButton()
    {
        bool allPassed = gradeService != null && gradeService.AllLevelsPassed();
        SetDiplomaButtonInteractable(allPassed);
    }

    private void SetDiplomaButtonInteractable(bool value)
    {
        if (diplomaButton != null)
            diplomaButton.interactable = value;
    }
}
