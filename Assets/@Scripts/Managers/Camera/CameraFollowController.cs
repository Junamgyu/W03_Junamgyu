using UnityEngine;

public class CameraFollowController : MonoBehaviour
{

    [SerializeField] private Transform target;
    [SerializeField] private Vector3 baseOffset;
    [SerializeField] private float smoothTime = 0.15f;

    private Vector3 currentVelocity;
    private Vector3 runtimeOffset;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetOffset(Vector3 offset)
    {
        runtimeOffset = offset;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + baseOffset + runtimeOffset;
        targetPos.z = transform.position.z;

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);
    }
}
