using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableTile : MonoBehaviour
{
    [SerializeField] private Tilemap _tilemap;
    public GameObject _fallingRock;
    private void Start()
    {
        _tilemap = GetComponent<Tilemap>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet == null) return;

        Vector3 hitPoint = collision.ClosestPoint(bullet.transform.position);
        Debug.Log("¸ÂÀ½");

        BreakTileAtWorld(hitPoint);
    }

    public void BreakTileAtWorld(Vector3 worldPosition)
    {
        Vector3Int cellPos = _tilemap.WorldToCell(worldPosition);

        if (!_tilemap.HasTile(cellPos))
            return;

        Vector3 spawnPosition = _tilemap.GetCellCenterWorld(cellPos);
        _tilemap.SetTile(cellPos, null);

        if (_fallingRock != null)
        {
            Instantiate(_fallingRock, spawnPosition, Quaternion.identity);
        }
    }
}
