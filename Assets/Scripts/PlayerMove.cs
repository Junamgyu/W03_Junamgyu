using UnityEngine;
using UnityEngine.Windows;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    Rigidbody2D _rb;
    Player player;
    Vector2 _dir;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        player.playerState._originalGravity = _rb.gravityScale;
    }

    void FixedUpdate()
    {
        _rb.AddForce(_dir * Time.deltaTime * player.playerState._moveSpeed, ForceMode2D.Impulse);

    }

    public void CanMove(Vector2 _input)
    {
        Vector2 _inputDir = new Vector2(_input.x, 0);
        _dir = _input;
        Debug.Log(_dir);

    }
}
