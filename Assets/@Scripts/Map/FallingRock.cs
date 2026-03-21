using UnityEngine;

public class FallingRock : MonoBehaviour
{
    [SerializeField] float _obejctDamage;

    /*
    private void OnTriggerStay2D(Collider other)
    {
        무적 시간을 불러와서 체크 한 번 더 해줘야 함.
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null && !_isInvincible)
        {
            playerHealth.TakeDamage(_obejctDamage);
        }
    }
    */
}
