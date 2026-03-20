using UnityEngine;

public abstract class EntityBase : MonoBehaviour, IDamageable
{
    // =====================
    // 공통 스탯
    // =====================
    [SerializeField] protected float _maxHp = 100f;
    [SerializeField] protected float _moveSpeed = 5f;
    [SerializeField] protected float _attackDamage = 10f;
    [SerializeField] protected float _knockBackForce = 5f;

    protected float _currentHp;
    protected Rigidbody2D _rb;

    // =====================
    // 프로퍼티
    // =====================
    public float CurrentHp => _currentHp;
    public float MaxHp => _maxHp;

    // =====================
    // 생명주기
    // =====================
    protected virtual void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        Initialize();
    }

    protected virtual void Initialize()
    {
        _currentHp = _maxHp;
    }

    // =====================
    // IDamageable 구현
    // =====================
    public virtual void TakeDamage(float damage)
    {
        if (_currentHp <= 0f) return;

        _currentHp -= damage;

        if (_currentHp <= 0f)
        {
            _currentHp = 0f;
            Die();
        }
    }

    public virtual void Die() { }
}