using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    Player player;

    Rigidbody2D _rb;
    Camera _cam;
    private void Start()
    {
        player = GetComponent<Player>();
        _rb = GetComponent<Rigidbody2D>();
        _cam = Camera.main;
    }

    public void Shotgun()
    {
        Vector2 mousePos = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 shootDir = (mousePos - (Vector2)transform.position).normalized;
        shootDir.x *= player.playerState._shootXMul;
        if (shootDir.y < 0)
        {
            shootDir.y = (shootDir.y - 1) / 2;
        }
        else
        {
            shootDir.y = (shootDir.y + 1) / 2;
        }

        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(-shootDir * player.playerState._shotgunForce, ForceMode2D.Impulse);

        Time.timeScale = 1f;
        StopCoroutine(nameof(GravityRoutine));
        StopCoroutine(nameof(DamplingRoutine));

        StartCoroutine(nameof(GravityRoutine));
        StartCoroutine(nameof(DamplingRoutine));

    }
    IEnumerator GravityRoutine()
    {
        _rb.gravityScale = 0f;
        yield return new WaitForSeconds(player.playerState._gravityOffDuration);
        _rb.gravityScale = player.playerState._originalGravity;
    }

    IEnumerator DamplingRoutine()
    {
        _rb.linearDamping = player.playerState._damplingValue;
        yield return new WaitForSeconds(player.playerState._damplingDuration);
        _rb.linearDamping = 0f;
    }
}
