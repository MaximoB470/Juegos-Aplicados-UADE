using UnityEngine;

/// <summary>
/// Maneja todos los sonidos del Nivel 3 (Quiz estilo ¿Quién quiere ser millonario?).
///
/// SETUP:
///  1. Agrega este componente al mismo GameObject que QuizUIManager.
///  2. Asigna los AudioClip desde el Inspector.
///  3. El script se suscribe automáticamente a los eventos de QuizGameManager y QuizUIManager.
///
/// SOURCES:
///  - sfxSource: efectos puntuales (clic, correcto, incorrecto, timeout).
///  - suspenseSource: AudioSource separado para el loop de suspenso.
///  - bgSource: AudioSource separado para la música de fondo en loop.
/// </summary>
public class QuizAudioManager : MonoBehaviour
{
    private const string GAME_MANAGER_KEY = "QuizGameManager";

    // ─── AudioSources ─────────────────────────────────────────────────────────

    [Header("Audio Sources")]
    [Tooltip("Para efectos puntuales (clic, correcto, incorrecto, timeout).")]
    [SerializeField] private AudioSource sfxSource;

    [Tooltip("Source dedicado al loop de suspenso. Loop = true en el Inspector.")]
    [SerializeField] private AudioSource suspenseSource;

    [Tooltip("Source para música de fondo en loop. Loop = true en el Inspector.")]
    [SerializeField] private AudioSource bgSource;

    // ─── AudioClips ───────────────────────────────────────────────────────────

    [Header("Clips — Interacción")]
    [Tooltip("Sonido al hacer clic en una opción (antes de ver el resultado).")]
    [SerializeField] private AudioClip clipClickOption;

    [Header("Clips — Suspenso y resultado")]
    [Tooltip("Loop que suena mientras se espera el resultado. Se detiene al revelar.")]
    [SerializeField] private AudioClip clipSuspense;

    [Tooltip("Fanfarria cuando la respuesta es correcta.")]
    [SerializeField] private AudioClip clipCorrect;

    [Tooltip("Sonido de fallo cuando la respuesta es incorrecta.")]
    [SerializeField] private AudioClip clipIncorrect;

    [Header("Clips — Estado del juego")]
    [Tooltip("Sonido cuando se acaba el tiempo sin responder.")]
    [SerializeField] private AudioClip clipTimeout;

    [Tooltip("Fanfarria al terminar el quiz.")]
    [SerializeField] private AudioClip clipQuizComplete;

    [Header("Clips — Música de fondo")]
    [Tooltip("Música de fondo que suena durante el quiz (opcional).")]
    [SerializeField] private AudioClip clipBgMusic;

    [Header("Volúmenes")]
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float suspenseVolume = 0.75f;
    [Range(0f, 1f)][SerializeField] private float bgVolume = 0.4f;

    // ─── Referencias ──────────────────────────────────────────────────────────

    private QuizGameManager gameManager;
    private QuizUIManager uiManager;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Start()
    {
        gameManager = ServiceLocator.Instance.GetService(GAME_MANAGER_KEY) as QuizGameManager;
        uiManager = ServiceLocator.Instance.GetService("QuizUIManager") as QuizUIManager;

        if (gameManager == null)
        {
            Debug.LogError("[QuizAudioManager] No se encontró QuizGameManager en el ServiceLocator.");
            return;
        }
        if (uiManager == null)
        {
            Debug.LogError("[QuizAudioManager] No se encontró QuizUIManager en el mismo GameObject.");
            return;
        }

        gameManager.OnSituationLoaded += HandleSituationLoaded;
        gameManager.OnAnswerResult += HandleAnswerResult;
        gameManager.OnTimeOut += HandleTimeOut;
        gameManager.OnQuizComplete += HandleQuizComplete;

        uiManager.OnResultRevealed += HandleResultRevealed;

        PlayBgMusic();
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnSituationLoaded -= HandleSituationLoaded;
            gameManager.OnAnswerResult -= HandleAnswerResult;
            gameManager.OnTimeOut -= HandleTimeOut;
            gameManager.OnQuizComplete -= HandleQuizComplete;
        }

        if (uiManager != null)
            uiManager.OnResultRevealed -= HandleResultRevealed;
    }

    // ─── Handlers de eventos ──────────────────────────────────────────────────

    private void HandleSituationLoaded(QuizSituationSO situation)
    {
        StopSuspense();
    }

    private void HandleAnswerResult(QuizOptionSO option, bool correct)
    {
        // Solo arranca el suspenso. El sonido de resultado lo dispara
        // OnResultRevealed, que viene del UI en el momento exacto en que
        // aparece el panel — sin ningún delay hardcodeado.
        PlaySuspense();
    }

    private void HandleResultRevealed(bool correct)
    {
        StopSuspense();
        PlaySFX(correct ? clipCorrect : clipIncorrect);
    }

    private void HandleTimeOut()
    {
        StopSuspense();
        PlaySFX(clipTimeout);
    }

    private void HandleQuizComplete(int correct, int total, float score)
    {
        StopSuspense();
        if (bgSource != null) bgSource.Pause();
        PlaySFX(clipQuizComplete);
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    /// <summary>Llamado por QuizUIManager al hacer clic en una opción.</summary>
    public void PlayClickOption()
    {
        PlaySFX(clipClickOption);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    private void PlaySuspense()
    {
        if (suspenseSource == null || clipSuspense == null) return;
        suspenseSource.clip = clipSuspense;
        suspenseSource.loop = true;
        suspenseSource.volume = suspenseVolume;
        suspenseSource.Play();
    }

    private void StopSuspense()
    {
        if (suspenseSource != null && suspenseSource.isPlaying)
            suspenseSource.Stop();
    }

    private void PlayBgMusic()
    {
        if (bgSource == null || clipBgMusic == null) return;
        bgSource.clip = clipBgMusic;
        bgSource.loop = true;
        bgSource.volume = bgVolume;
        bgSource.Play();
    }
}