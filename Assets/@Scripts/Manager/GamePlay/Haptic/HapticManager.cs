using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class HapticManager : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    private Coroutine _hapticRoutine;

    public void Initialize()
    {
        if (IsInitialized)
            return;

        IsInitialized = true;
    }

    public void PlayOneShot(float lowFrequency, float highFrequency, float duration)
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null)
            return;

        if (_hapticRoutine != null)
            StopCoroutine(_hapticRoutine);

        _hapticRoutine = StartCoroutine(CoPlayOneShot(gamepad, lowFrequency, highFrequency, duration));
    }

    public void PlayPistolShot()
    {
        PlayOneShot(0.15f, 0.35f, 0.08f);
    }

    public void PlayShotgunShot()
    {
        PlayOneShot(0.45f, 0.8f, 0.12f);
    }

    public void PlayPlayerHit()
    {
        PlayOneShot(0.6f, 1f, 0.15f);
    }

    public void StopHaptics()
    {
        if (_hapticRoutine != null)
        {
            StopCoroutine(_hapticRoutine);
            _hapticRoutine = null;
        }

        if (Gamepad.current != null)
            Gamepad.current.ResetHaptics();
    }

    private IEnumerator CoPlayOneShot(Gamepad gamepad, float lowFrequency, float highFrequency, float duration)
    {
        gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
        yield return new WaitForSeconds(duration);

        if (gamepad != null)
            gamepad.ResetHaptics();

        _hapticRoutine = null;
    }

    private void OnDisable()
    {
        StopHaptics();
    }
}