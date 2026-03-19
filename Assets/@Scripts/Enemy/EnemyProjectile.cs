using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private float _speed;
    private float _damage;
    [SerializeField] private float _lifetime = 5f;

    public void Initialize(float speed, float damage)
    {
        _speed = speed;
        _damage = damage;
        Destroy(gameObject, _lifetime);
    }

    void Update()
    {
        // rotation이 이미 방향을 가리키고 있으니 right(로컬 x축)으로 이동
        transform.Translate(Vector2.right * _speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            //col.GetComponent<Player>().TakeDamage(_damage);
            Destroy(gameObject);
        }

        if (col.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}