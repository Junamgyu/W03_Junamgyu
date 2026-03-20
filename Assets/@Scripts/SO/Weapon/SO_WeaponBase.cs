using UnityEngine;

[CreateAssetMenu(fileName = "SO_WeaponBase", menuName = "Scriptable Objects/SO_WeaponBase")]
public class SO_WeaponBase : ScriptableObject
{
    public string weaponName;

    [Header("Recoil")]
    public float recoilForce;
    
    [Header("Bullet")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public int pelletCount = 1;
    public float spreadAngle = 0f;
    public int maxAmmo = 3;

}

