using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowManager : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    [SerializeField] private string _currentStage;

    public string CurrentStage => _currentStage;
    public bool IsLoading { get; private set; }

    public event Action<string> OnStageReloadStarted;
    public event Action<string> OnStageReloadCompleted;
    public event Action<string> OnStageLoaded;

    public void Initialize()
    {
        if (IsInitialized)
            return;

        if (string.IsNullOrEmpty(_currentStage))
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid())
                _currentStage = activeScene.name;
        }

        IsInitialized = true;
    }

    public void SetCurrentStage(string stageName)
    {
        if (string.IsNullOrEmpty(stageName))
            return;

        _currentStage = stageName;
    }

    public void LoadStage(string stageName)
    {
        if (IsLoading)
            return;

        if (string.IsNullOrEmpty(stageName))
        {
            Debug.LogWarning($"{name}: stageName is null or empty.");
            return;
        }

        StartCoroutine(CoLoadStage(stageName));
    }

    public void ReloadStage()
    {
        if (IsLoading)
            return;

        if (string.IsNullOrEmpty(_currentStage))
        {
            Debug.LogWarning($"{name}: CurrentStage is empty.");
            return;
        }

        StartCoroutine(CoReloadStage(_currentStage));
    }

    private IEnumerator CoLoadStage(string stageName)
    {
        IsLoading = true;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(stageName, LoadSceneMode.Additive);
        if (loadOperation == null)
        {
            IsLoading = false;
            yield break;
        }

        yield return loadOperation;

        Scene loadedScene = SceneManager.GetSceneByName(stageName);
        if (loadedScene.IsValid() && loadedScene.isLoaded)
        {
            SceneManager.SetActiveScene(loadedScene);
            _currentStage = stageName;
            OnStageLoaded?.Invoke(stageName);
        }

        IsLoading = false;
    }

    private IEnumerator CoReloadStage(string stageName)
    {
        IsLoading = true;
        OnStageReloadStarted?.Invoke(stageName);

        Scene scene = SceneManager.GetSceneByName(stageName);

        if (scene.IsValid() && scene.isLoaded)
        {
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(stageName);
            if (unloadOperation != null)
                yield return unloadOperation;
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(stageName, LoadSceneMode.Additive);
        if (loadOperation != null)
            yield return loadOperation;

        Scene loadedScene = SceneManager.GetSceneByName(stageName);
        if (loadedScene.IsValid() && loadedScene.isLoaded)
        {
            SceneManager.SetActiveScene(loadedScene);
            _currentStage = stageName;
            OnStageReloadCompleted?.Invoke(stageName);
        }

        IsLoading = false;
    }
}