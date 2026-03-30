using UnityEngine;
using UnityEngine.UI;
public class PlayerHpUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private Slider _hpSlider;

    private void Start()
    {
        _playerHealth.OnHit += OnHit;
        _playerHealth.OnHeal += OnHeal;

        StartCoroutine(InitRoutine());
    }

    private void OnDestory()
    {
        _playerHealth.OnHit -= OnHit;
        _playerHealth.OnHeal -= OnHeal;
    }

    System.Collections.IEnumerator InitRoutine()
    {
        yield return null;

        if(_hpSlider != null)
        {
            _hpSlider.minValue = 0;
            _hpSlider.maxValue = _playerHealth.MaxHp;
            _hpSlider.value = _playerHealth.CurrentHp;
        }
    }

    void OnHit(int _) => UpdateUI();
    void OnHeal(int _) => UpdateUI();

    void UpdateUI()
    {
        if(_hpSlider != null)
            _hpSlider.value = _playerHealth.CurrentHp;
    }
    
}
