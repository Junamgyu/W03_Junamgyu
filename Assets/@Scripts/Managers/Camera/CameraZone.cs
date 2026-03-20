using UnityEngine;

public class CameraZone : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private bool useBounds;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    public Vector3 Offset => offset;
    public bool UseBounds => useBounds;
    public Vector2 MinBounds => minBounds;
    public Vector2 MaxBounds => maxBounds;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        CameraManager.Instance.SetZone(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        CameraManager.Instance.ClearZone(this);
    }
}
