using System.Collections;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{

    [Header("설정")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _respawnDelay = 1.5f;
    [SerializeField] private PlayerHealth _playerHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private void OnDestory()
    {
        _playerHealth.OnDie -= OnPlayerDie;
    }

    void OnPlayerDie()
    {
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        _playerHealth.gameObject.SetActive(true);
        yield return new WaitForSeconds(_respawnDelay);

        _playerHealth.transform.position = _spawnPoint.position;

        _playerHealth.ResetHP();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
