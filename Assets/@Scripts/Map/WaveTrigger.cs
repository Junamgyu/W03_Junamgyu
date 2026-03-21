using UnityEngine;

public class WaveTrigger : MonoBehaviour
{
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private GameObject[] _moveBlocks;

    private bool _triggered;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_triggered)
            return;

        Player player = collision.GetComponent<Player>();
        if (player == null)
            return;

        _triggered = true;

        if (_enemySpawner != null)
        {
            _enemySpawner.StartFirstWave();
        }

        for (int i = 0; i < _moveBlocks.Length; i++)
        {
            if (_moveBlocks[i] != null)
            {
                _moveBlocks[i].SetActive(true);
            }
        }

        Destroy(gameObject);
    }
}