using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    [SerializeField] private int levelSceneBuildIndex = 1;

    public void GoToLevel()
    {
        SceneManager.LoadScene(levelSceneBuildIndex);
    }
}