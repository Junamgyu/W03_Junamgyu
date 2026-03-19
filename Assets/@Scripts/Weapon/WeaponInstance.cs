using UnityEngine;

public class WeaponInstance : MonoBehaviour
{
    public SO_WeaponBase Data { get; private set; }
    public int CurrentAmmo { get; private set; }

    public WeaponInstance(SO_WeaponBase data)
    {
        Data = data;
        CurrentAmmo = data.maxAmmo;
    }

    public bool TryConsume()
    {
        if (CurrentAmmo <= 0) return false;
        CurrentAmmo--;
        return true;
    }

    public void Reload()
    {
        CurrentAmmo = Data.maxAmmo;
    }
}
