using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu_UI : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private String _gameSceneName = "Level1";

    private void Awake()
    {
        _startButton?.onClick.AddListener(OnStartClicked);
        _quitButton?.onClick.AddListener(OnQuitClicked);
    }

    void OnStartClicked()
    {
        if(ManagerRegistry.TryGet<SceneFlowManager>(out var sceneFlow))
        {
            sceneFlow.SetCurrentStage(_gameSceneName);
            sceneFlow.LoadStage();
        }
        else SceneManager.LoadScene(_gameSceneName);
    }

    private void OnDestroy()
    {
        _startButton?.onClick.RemoveAllListeners();
        _quitButton?.onClick.RemoveAllListeners();
    }

    void OnQuitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 플레이 중지
        #else
            Application.Quit(); // 빌드에서 게임 종료
        #endif
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
