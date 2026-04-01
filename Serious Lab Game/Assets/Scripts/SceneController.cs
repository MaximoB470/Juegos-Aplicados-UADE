using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    public enum GameResult
    {
        None,
        Win,
        Lose
    }

    public GameResult LastResult = GameResult.None;

    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "ProtoGameScene";
    [SerializeField] private string menuSceneName = "MenuScene";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ServiceLocator.Instance.SetService("SceneController", this);
    }

    public void StartGame()
    {
        LastResult = GameResult.None;
        SceneManager.LoadScene(gameSceneName);
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }


    public void SetWin()
    {
        LastResult = GameResult.Win;
        ReloadScene();
    }

    public void SetLose()
    {
        LastResult = GameResult.Lose;
        ReloadScene();
    }
}