using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowManager : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    private string _currentStage;

    public void Initialize()
    {
        if (IsInitialized) return;

        IsInitialized = true;
    }

    public void LoadStage(string stageName)
    {
        if (!string.IsNullOrEmpty(_currentStage))
        {
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_currentStage);
        }

        _currentStage = stageName;
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(stageName, LoadSceneMode.Additive);
    }

    public void ReloadStage()
    {
        if (string.IsNullOrEmpty(_currentStage)) return;

        LoadStage(_currentStage);
    }
}