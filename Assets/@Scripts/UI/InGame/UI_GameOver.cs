using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UI_GameOver : MonoBehaviour
{
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;

    private PauseController _pauseController;
    private InputManager _inputManager;

    public event System.Action OnRetryRequested;
    public event System.Action OnMainMenuRequested;

    private Button[] _buttons;
    private int _currentIndex;
    private float _lastNavigateTime;
    private const float NavigateCooldown = 0.2f;

    private bool _isPointerMode = true;

    private void Awake()
    {
        ManagerRegistry.TryGet(out _pauseController);
        ManagerRegistry.TryGet(out _inputManager);

        _buttons = new Button[]
        {
            _restartButton,
            _mainMenuButton
        };

        BindButtons();
    }

    private void OnEnable()
    {
        SetPointerMode();

        if (_inputManager != null)
        {
            _inputManager.OnNavigate += HandleNavigate;
            _inputManager.OnSubmit += HandleSubmit;
        }
    }

    private void OnDisable()
    {
        if (_inputManager != null)
        {
            _inputManager.OnNavigate -= HandleNavigate;
            _inputManager.OnSubmit -= HandleSubmit;
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
        if (_restartButton != null)
            _restartButton.onClick.AddListener(HandleClickRetry);

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(HandleClickMainMenu);
    }

    private void UnbindButtons()
    {
        if (_restartButton != null)
            _restartButton.onClick.RemoveListener(HandleClickRetry);

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.RemoveListener(HandleClickMainMenu);
    }

    private void HandleNavigate(InputAction.CallbackContext ctx)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (!ctx.performed)
            return;

        if (Time.unscaledTime - _lastNavigateTime < NavigateCooldown)
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
        _currentIndex = GetFirstValidButtonIndex();

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