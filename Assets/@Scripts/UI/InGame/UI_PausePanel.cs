using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UI_PausePanel : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _mainMenuButton;

    private PauseController _pauseController;

    public event Action OnRetryRequested;
    public event Action OnMainMenuRequested;

    private void Awake()
    {
        ManagerRegistry.TryGet(out _pauseController);
        BindButtons();
    }

    private void OnEnable()
    {
        SelectDefaultButton();
    }

    private void OnDestroy()
    {
        UnbindButtons();
    }

    private void BindButtons()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.AddListener(HandleClickResume);

        if (_retryButton != null)
            _retryButton.onClick.AddListener(HandleClickRetry);

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(HandleClickMainMenu);
    }

    private void UnbindButtons()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.RemoveListener(HandleClickResume);

        if (_retryButton != null)
            _retryButton.onClick.RemoveListener(HandleClickRetry);

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.RemoveListener(HandleClickMainMenu);
    }

    private void SelectDefaultButton()
    {
        if (EventSystem.current == null || _resumeButton == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_resumeButton.gameObject);
    }

    private void HandleClickResume()
    {
        _pauseController?.ResumeGame();
    }

    private void HandleClickRetry()
    {
        OnRetryRequested?.Invoke();
    }

    private void HandleClickMainMenu()
    {
        OnMainMenuRequested?.Invoke();
    }
}