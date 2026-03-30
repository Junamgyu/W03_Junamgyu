using UnityEngine;

public class ShieldHitBox : MonoBehaviour
{
    private PlayerShield _shield;

    private void Awake()
    {
        _shield = GetComponentInParent<PlayerShield>();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(_shield == null) return;
        if(!_shield.IsShieldActive) return;

        //보스 본체, 보스 패턴 칼, 낙하 칼 등 Enemy 태그/레이어 처리
        bool isEnemyAttack =
            other.gameObject.layer == LayerMask.NameToLayer("Enemy") ||
            other.CompareTag("Enemy") ||
            other.GetComponent<Level01_BossOrbitSword>() != null;

        if(isEnemyAttack)
        {
               Debug.Log("방패로 막음");
        }
    }
}
