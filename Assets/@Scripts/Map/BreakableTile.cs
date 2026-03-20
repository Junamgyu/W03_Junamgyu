using UnityEngine;

public class BreakableTile : MonoBehaviour
{
    public GameObject _fallingRock;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Bullet _bullet = collision.GetComponent<Bullet>();
        if (_bullet != null)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        Instantiate(_fallingRock, transform.position, Quaternion.identity);
    }

    public void Test()
    {
        Destroy(gameObject);
    }
}
