using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimer : MonoBehaviour
{
    [SerializeField] private Camera _cam;

    public Vector2 AimDirection { get; private set; } = Vector2.right;

    void Awake()
    {
        if (_cam == null) _cam = Camera.main;
    }

    void Update()
    {
        Vector2 mouseWorld = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dir = mouseWorld - (Vector2)transform.position;
        if (dir.sqrMagnitude > 0.001f)
            AimDirection = dir.normalized;
    }
}
