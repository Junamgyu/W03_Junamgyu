using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowManager : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    [SerializeField] private string _currentStageSceneName;
    public string CurrentStageSceneName => _currentStageSceneName;

    [SerializeField] private string _mainMenuSceneName = "MainMenu";

    public bool IsLoading { get; private set; }

    public event Action<string> OnStageReloadCompleted;

    public void Initialize()
    {
        if (IsInitialized)
            return;

        if (string.IsNullOrEmpty(_currentStageSceneName))
        {
            Scene activeScene = SceneManager.GetActiveScene();
            _currentStageSceneName = activeScene.name;
        }

        SceneManager.sceneLoaded += HandleSceneLoaded;
        IsInitialized = true;
    }

    public void SetCurrentStage(string stageSceneName)
    {
        if (string.IsNullOrEmpty(stageSceneName))
            return;

        _currentStageSceneName = stageSceneName;
    }

    public void ReloadStage()
    {
        if (IsLoading)
            return;

        if (string.IsNullOrEmpty(_currentStageSceneName))
        {
            Debug.LogWarning($"{name}: CurrentStageSceneName is empty.");
            return;
        }

        IsLoading = true;
        SceneManager.LoadScene(_currentStageSceneName, LoadSceneMode.Single);
    }

    public void LoadMainMenu()
    {
        if (IsLoading)
            return;

        if (string.IsNullOrEmpty(_mainMenuSceneName))
        {
            Debug.LogWarning($"{name}: MainMenuSceneName is empty! Check SceneFlowManager.");
            return;
        }

        Time.timeScale = 1f;
        IsLoading = true;
        SceneManager.LoadScene(_mainMenuSceneName, LoadSceneMode.Single);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsLoading)
            return;

        if (scene.name != _currentStageSceneName)
            return;

        IsLoading = false;
        OnStageReloadCompleted?.Invoke(scene.name);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }
}