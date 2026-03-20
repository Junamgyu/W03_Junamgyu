using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _lifetime = 1f;
    [SerializeField] private bool _isPiercing = false; // 관통 여부
    [SerializeField] private bool _giveGauge = true;

    private Coroutine _lifeRoutine;
    private Rigidbody2D _rb;
    private PoolManager _pool;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        ManagerRegistry.TryGet(out _pool);
    }

    private void OnEnable()
    {
        if (_lifeRoutine != null)
        {
            StopCoroutine(_lifeRoutine);
        }

        _lifeRoutine = StartCoroutine(LifeReturnRoutine(_pool));
    }

    private void OnDisable()
    {
        if (_lifeRoutine != null)
        {
            StopCoroutine(_lifeRoutine);
            _lifeRoutine = null;
        }

        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }
    }

    #region Pool Return 코루틴
    private IEnumerator LifeReturnRoutine(PoolManager pool)
    {
        yield return new WaitForSeconds(_lifetime);

        if (pool != null)
        {
            pool.Return(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (_isPiercing) return; // 관통이면 무시
            ReturnToPool();
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (other.TryGetComponent<EnemyBase>(out var damageable))
                damageable.TakeDamage(_damage, _giveGauge);

            ReturnToPool();
        }
    }

    //Helper
    private void ReturnToPool()
    {
        if (ManagerRegistry.TryGet<PoolManager>(out var pool))
            pool.Return(gameObject);
        else
            Destroy(gameObject);
    }
}