using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] private float _maxHp = 100f;
    private float _currentHp;

    [SerializeField] private GameObject _markIndicator; // 마킹 표시용 오브젝트

    void Start()
    {
        _currentHp = _maxHp;
    }

    public void TakeDamage(float damage)
    {
        _currentHp -= damage;
        if (_currentHp <= 0)
            Die();
    }

    public void ShowMark(bool show)
    {
        if (_markIndicator != null)
            _markIndicator.SetActive(show);
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
