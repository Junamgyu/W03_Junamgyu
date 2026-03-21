using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UI_Pause : MonoBehaviour
{
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _mainMenuButton;

    private PauseController _pauseController;

    public event System.Action OnRetryRequested;
    public event System.Action OnMainMenuRequested;

    private void Awake()
    {
        ManagerRegistry.TryGet(out _pauseController);
        BindButtons();
    }

    private void OnEnable()
    {
        StartCoroutine(CoSelectDefaultButton());
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

    private IEnumerator CoSelectDefaultButton()
    {
        yield return null;

        if (EventSystem.current == null)
            yield break;

        Button targetButton = _resumeButton != null ? _resumeButton : _retryButton;
        if (targetButton == null)
            yield break;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(targetButton.gameObject);
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