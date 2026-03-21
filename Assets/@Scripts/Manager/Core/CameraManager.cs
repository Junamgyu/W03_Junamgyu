using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    public enum CameraMode
    {
        Follow,
        ZoneFollow,
        Locked,
        Boss,
    }
    [SerializeField] private CameraMode _cameraMode;

    //[SerializeField] private CameraFollowController followController;
    [SerializeField] private CameraBoundsController boundsController;
    //[SerializeField] private CameraEffectController effectController;
    [SerializeField] private CameraZoneController zoneController;

    public CameraZone CurrentZone { get; private set; }
    public CameraMode CurrentMode { get; private set; } = CameraMode.Follow;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }else
        {
            Destroy(gameObject);
        }
    }

    public void SetZone(CameraZone zone)
    {
        CurrentZone = zone;
        CurrentMode = zone != null ? CameraMode.ZoneFollow : CameraMode.Follow;

        zoneController.ApplyZone(zone);
    }

    public void ClearZone(CameraZone zone)
    {
        if (CurrentZone != zone) return;

        CurrentZone = null;
        CurrentMode = CameraMode.Follow;

        zoneController.ApplyZone(null);
    }
}
