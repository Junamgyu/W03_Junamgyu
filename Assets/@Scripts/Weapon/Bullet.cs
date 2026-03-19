using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _lifetime = 2f;

    private void Start()
    {
        Destroy(gameObject, _lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // "Enemy" 태그가 있는 오브젝트에 명중 시 제거
        // 이후 IDamageable 인터페이스로 확장하기 좋은 자리
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground") || other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
