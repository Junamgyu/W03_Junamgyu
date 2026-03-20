using UnityEngine;

public class WeaponInstance
{
    public SO_WeaponBase Data { get; private set; }
    public int CurrentAmmo { get; private set; }
    private float _nextFireTime = 0f;

    public bool IsReady => Time.time >= _nextFireTime;

    public WeaponInstance(SO_WeaponBase data)
    {
        Data = data;
        CurrentAmmo = data.maxAmmo;
    }

    public bool TryConsume()
    {
        if (!IsReady) return false;
        if (CurrentAmmo <= 0) return false;

        CurrentAmmo--;
        _nextFireTime = Time.time + Data.fireRate;
        return true;
    }

    public void Reload()
    {
        CurrentAmmo = Data.maxAmmo;
    }
}
