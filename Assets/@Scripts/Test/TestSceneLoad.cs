using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TestSceneLoad : MonoBehaviour
{
    public void GameStart(string _sceneName)
    {
        SceneManager.LoadScene(_sceneName);
    }
}
