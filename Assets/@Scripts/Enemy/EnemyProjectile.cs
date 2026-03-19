using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Vector2 _direction;
    private float _speed;
    private float _damage;
    [SerializeField] private float _lifetime = 5f;  // 최대 생존 시간

    public void Initialize(Vector2 direction, float speed, float damage)
    {
        _direction = direction;
        _speed = speed;
        _damage = damage;
        Destroy(gameObject, _lifetime);
    }

    void Update()
    {
        transform.Translate(_direction * _speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            //col.GetComponent<Player>().TakeDamage(_damage);
            Destroy(gameObject);
        }

        // 벽에 맞으면 제거
        if (col.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}