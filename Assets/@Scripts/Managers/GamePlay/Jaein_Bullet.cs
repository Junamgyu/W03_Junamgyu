using System.Collections;
using UnityEngine;

public class Jaein_Bullet : MonoBehaviour
{
    [SerializeField] private float _lifetime = 2f;

    private void Start()
    {
        if (ManagerRegistry.TryGet<PoolManager>(out var pool))
        {
            StartCoroutine(LifeReturnRoutine(pool));
        }
        else
        {
            Destroy(gameObject, _lifetime);
        }
    }

    #region Pool Return 코루틴
    private System.Collections.IEnumerator LifeReturnRoutine(PoolManager pool)
    {
        yield return new WaitForSeconds(_lifetime);

        // 풀에 반환
        pool.Return(gameObject);
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground") || other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
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