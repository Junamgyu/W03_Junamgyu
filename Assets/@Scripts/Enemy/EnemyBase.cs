using System;
using System.Collections;
using UnityEngine;

public abstract class EnemyBase : EntityBase
{
    // 이벤트
    public event Action<EnemyBase> OnDeathFinished;

    // =====================
    // 마킹
    // =====================
    [SerializeField] private CircleDrawer _markIndicator;
    private bool _isMarked = false;

    public void ShowMark(bool show)
    {
        if (_markIndicator == null) return;
        _isMarked = show;
        _markIndicator.gameObject.SetActive(show);
    }

    public bool IsMarked() => _isMarked;

    // =====================
    // TakeDamage
    // =====================
    public virtual void TakeDamage(int damage, bool isAddGauge = false)
    {
        base.TakeDamage(damage);
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
    }

    public override void Die() { }

    // =====================
    // 사망 루틴 (공통)
    // =====================
    protected IEnumerator DieRoutine()
    {
        yield return StartCoroutine(OnDieRoutine());
        OnDeathFinished?.Invoke(this);
    }

    protected virtual IEnumerator OnDieRoutine()
    {
        yield break;
    }
}