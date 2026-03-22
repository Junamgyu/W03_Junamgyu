using System.Collections;
using UnityEngine;

public class ParticleAutoReturn : MonoBehaviour
{
    private PoolManager _pool;
    private ParticleSystem _ps;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
        ManagerRegistry.TryGet(out _pool);
    }

    private void OnEnable()
    {
        StartCoroutine(ReturnRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator ReturnRoutine()
    {
        yield return new WaitForSeconds(_ps.main.duration + _ps.main.startLifetime.constantMax);
        if (_pool != null)
            _pool.Return(gameObject);
        else
            Destroy(gameObject);
    }

}
