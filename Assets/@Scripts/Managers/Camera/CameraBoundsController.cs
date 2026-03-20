using UnityEngine;

public class CameraBoundsController : MonoBehaviour
{
    private bool useBounds;
    private Vector2 minBounds;
    private Vector2 maxBounds;

    public void SetBounds(Vector2 min, Vector2 max, bool enabled)
    {
        minBounds = min;
        maxBounds = max;
        useBounds = enabled;
    }

    public void ClearBounds()
    {
        useBounds = false;
    }

    private void LateUpdate()
    {
        if (!useBounds) return;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
        transform.position = pos;
    }
}
