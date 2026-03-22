using UnityEngine;

public class Explosives : MonoBehaviour
{
    [SerializeField] LayerMask _interactionMask;
    [SerializeField] float _explosionRadius;
    [SerializeField] int _explosionDamage;

    [Header("Effect")]
    [SerializeField] private GameObject _explosionParticlePrefab;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null)
        {
            Destroy(gameObject);
        }
    }

    public void OnDestroy()
    {
        Explosion();
    }

    public void Explosion()
    {
        SpawnExplosionParticle();

        Collider2D[] _hits = Physics2D.OverlapCircleAll(transform.position, _explosionRadius, _interactionMask);
        foreach (Collider2D _hit in _hits)
        {
            PlayerHealth playerHealth;
            BreakableTile tile;
            EnemyBase _enemy;
            
            if((tile = _hit.GetComponent<BreakableTile>()) != null)
            {
                Destroy(tile.gameObject);
            }else if((_enemy = _hit.GetComponent<EnemyBase>())!= null)
            {
                _enemy.TakeDamage(_explosionDamage *100);
            }
            else if ((playerHealth = _hit.gameObject.GetComponent<PlayerHealth>()) != null)
            {
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(_explosionDamage);
                }
            }
        }
    }

    private void SpawnExplosionParticle()
    {
        if (_explosionParticlePrefab == null) return;

        GameObject go = Instantiate(_explosionParticlePrefab, transform.position, Quaternion.identity);

        // 폭발 반경에 맞게 스케일 조절
        float scale = _explosionRadius * 0.2f;
        go.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void Test()
    {
        Explosion();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
}
