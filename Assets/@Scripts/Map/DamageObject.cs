using UnityEngine;

public class DamageObject : MonoBehaviour
{
    [SerializeField] int _obejctDamage;

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(_obejctDamage);
        }
    }
}
