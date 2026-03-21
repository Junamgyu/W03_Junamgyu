using UnityEngine;

public class DamageObject : MonoBehaviour
{
    [SerializeField] int _obejctDamage;

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(_obejctDamage);
        }
    }
}
