using UnityEngine;

public class Level01_BossOrbitSword : MonoBehaviour
{
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _damageCooldown = 0.5f;
    private float _lastDamageTime = -999f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(Time.time - _lastDamageTime < _damageCooldown) return;

        if(other.CompareTag("Player"))
        {
            other.GetComponent<IDamageable>()?.TakeDamage(_damage);
            _lastDamageTime = Time.time;
        }
    }
}
