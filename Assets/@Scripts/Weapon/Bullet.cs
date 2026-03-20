using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _lifetime = 1f;

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
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground") ||
            other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (ManagerRegistry.TryGet<PoolManager>(out var pool))
            {
                pool.Return(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}