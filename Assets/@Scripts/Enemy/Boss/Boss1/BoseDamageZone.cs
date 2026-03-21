using UnityEngine;

public class BoseDamageZone : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            other.GetComponent<IDamageable>()?.TakeDamage(damage);
    }
}