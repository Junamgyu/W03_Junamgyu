using UnityEngine;

public class WaveTrigger : MonoBehaviour
{
    public EnemySpawner _enemySpawner;
    public GameObject[] _moveBlocks;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if(player != null)
        {
            _enemySpawner.StartFirstWave();
            Destroy(gameObject);
        }
    }

    public void OnDestroy()
    {

        for (int i = 0; i < _moveBlocks.Length; i++)
        {
           
            _moveBlocks[i].SetActive(true);
        }
    }
}
