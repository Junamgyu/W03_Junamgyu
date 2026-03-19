using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    private InputSystem_Actions _input;

    public event Action<Vector2> OnMove;
    public event Action OnJump;
    public event Action OnPrimaryAttack;
    public event Action OnSecondaryAttack;
    public event Action OnSkill;
    public event Action OnInteract;

    public void Initialize()
    {
        if (IsInitialized) return;

        _input = new InputSystem_Actions();

        BindActions();
        _input.Enable();

        IsInitialized = true;
    }

    private void BindActions()
    {
        var player = _input.Player;

        // Move (Vector2)
        player.Move.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
        player.Move.canceled += ctx => OnMove?.Invoke(Vector2.zero);

        // Jump
        player.Jump.performed += _ => OnJump?.Invoke();

        // Attack + Shotgun (Primary + Secondary Attack)
        player.Attack.performed += _ => OnPrimaryAttack?.Invoke();
        player.Shotgun.performed += _ => OnSecondaryAttack?.Invoke(); // 추가된 공격 액션

        // Interact (Optional)
        // player.Interact.performed += _ => OnInteract?.Invoke();

        // Skill (Deadeye)
        player.Skill.started += _ => OnSkill?.Invoke();
        player.Skill.canceled += _ => OnSkill?.Invoke();
    }

    private void OnDestroy()
    {
        if (_input != null)
        {
            _input.Disable();
        }
    }
}