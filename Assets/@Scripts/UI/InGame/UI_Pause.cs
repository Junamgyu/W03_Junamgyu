using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UI_Pause : MonoBehaviour
{
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _mainMenuButton;

    private PauseController _pauseController;
    private InputManager _inputManager;

    public event System.Action OnRetryRequested;
    public event System.Action OnMainMenuRequested;

    private Button[] _buttons;
    private int _currentIndex;
    private float _navigateCooldown = 0.2f;
    private float _lastNavigateTime;

    private bool _isPointerMode = true;

    private void Awake()
    {
        ManagerRegistry.TryGet(out _pauseController);
        ManagerRegistry.TryGet(out _inputManager);

        _buttons = new Button[]
        {
            _resumeButton,
            _retryButton,
            _mainMenuButton
        };

        BindButtons();
    }

    private void OnEnable()
    {
        _currentIndex = GetFirstValidButtonIndex();
        SetPointerMode();

        if (_inputManager != null)
        {
            _inputManager.OnNavigate += HandleNavigate;
            _inputManager.OnSubmit += HandleSubmit;
            _inputManager.OnCancel += HandleCancel;
        }
    }

    private void OnDisable()
    {
        if (_inputManager != null)
        {
            _inputManager.OnNavigate -= HandleNavigate;
            _inputManager.OnSubmit -= HandleSubmit;
            _inputManager.OnCancel -= HandleCancel;
        }
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (Mouse.current == null)
            return;

        bool pointerUsed =
            Mouse.current.delta.ReadValue().sqrMagnitude > 0f ||
            Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame;

        if (pointerUsed && !_isPointerMode)
        {
            SetPointerMode();
        }
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

    private void HandleNavigate(InputAction.CallbackContext ctx)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (!ctx.performed)
            return;

        if (Time.unscaledTime - _lastNavigateTime < _navigateCooldown)
            return;

        Vector2 input = ctx.ReadValue<Vector2>();

        if (Mathf.Abs(input.y) < 0.5f)
            return;

        SetNavigationMode();

        if (input.y > 0.5f)
            MoveSelection(-1);
        else if (input.y < -0.5f)
            MoveSelection(1);
    }

    private void HandleSubmit(InputAction.CallbackContext ctx)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (!ctx.performed)
            return;

        if (_isPointerMode)
            return;

        Button currentButton = GetCurrentButton();
        if (currentButton == null)
            return;

        currentButton.onClick.Invoke();
    }

    private void HandleCancel(InputAction.CallbackContext ctx)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (!ctx.performed)
            return;

        if (_isPointerMode)
            return;

        HandleClickResume();
    }

    private void MoveSelection(int direction)
    {
        if (_buttons == null || _buttons.Length == 0)
            return;

        int startIndex = _currentIndex;

        do
        {
            _currentIndex += direction;

            if (_currentIndex < 0)
                _currentIndex = _buttons.Length - 1;
            else if (_currentIndex >= _buttons.Length)
                _currentIndex = 0;

            if (IsButtonValid(_buttons[_currentIndex]))
            {
                _lastNavigateTime = Time.unscaledTime;
                SyncEventSystemSelection();
                return;
            }
        }
        while (_currentIndex != startIndex);
    }

    private void SetPointerMode()
    {
        _isPointerMode = true;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void SetNavigationMode()
    {
        if (!_isPointerMode)
            return;

        _isPointerMode = false;

        if (!IsButtonValid(GetCurrentButton()))
            _currentIndex = GetFirstValidButtonIndex();

        SyncEventSystemSelection();
    }

    private void SyncEventSystemSelection()
    {
        if (EventSystem.current == null)
            return;

        Button currentButton = GetCurrentButton();
        if (currentButton == null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            return;
        }

        EventSystem.current.SetSelectedGameObject(currentButton.gameObject);
    }

    private int GetFirstValidButtonIndex()
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            if (IsButtonValid(_buttons[i]))
                return i;
        }

        return 0;
    }

    private bool IsButtonValid(Button button)
    {
        return button != null && button.gameObject.activeInHierarchy && button.interactable;
    }

    private Button GetCurrentButton()
    {
        if (_buttons == null || _buttons.Length == 0)
            return null;

        if (_currentIndex < 0 || _currentIndex >= _buttons.Length)
            return null;

        if (!IsButtonValid(_buttons[_currentIndex]))
            return null;

        return _buttons[_currentIndex];
    }

    private void HandleClickResume()
    {
        SetPointerMode();
        _pauseController?.ResumeGame();
    }

    private void HandleClickRetry()
    {
        SetPointerMode();
        OnRetryRequested?.Invoke();
    }

    private void HandleClickMainMenu()
    {
        SetPointerMode();
        OnMainMenuRequested?.Invoke();
    }
}