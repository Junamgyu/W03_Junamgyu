using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : EntityBase
{

    [Header("Invincibility")]
    [SerializeField] private float _invincibleDuration = 1.5f;
    private bool _isInvincible = false;


    public bool IsInvincible => _isInvincible;

    public event Action<int> OnHit; // Amount   
    public event Action<int> OnHeal;
    public event Action OnDie;


    public override void TakeDamage(int damage)
    {
        if (_isInvincible) return;
        if (_currentHp <= 0f) return;

        base.TakeDamage(damage);

        Debug.Log("currentHp: " + _currentHp + ", damage: " + damage);

        OnHit?.Invoke(damage);

        if (_currentHp > 0)
            StartCoroutine(InvincibleRoutine());
    }

    public void Heal(int amount = 1)
    {
        if (_currentHp <= 0f) return;

        int actual = Mathf.Min(amount, _maxHp - _currentHp);
        if (actual <= 0) return; // 이미 풀피

        _currentHp += actual;
        OnHeal?.Invoke(actual);
    }

    public override void Die()
    {
        _isInvincible = false;
        StopAllCoroutines();
        OnDie?.Invoke();
        base.Die();
    }

    private IEnumerator InvincibleRoutine()
    {
        _isInvincible = true;
        yield return new WaitForSeconds(_invincibleDuration);
        _isInvincible = false;
    }
}
