using System;
using UnityEngine;

public class WeaponInstance
{
    public SO_WeaponBase Data { get; private set; }
    public int CurrentAmmo { get; private set; }
    private float _nextFireTime = 0f;
    private float _reloadEndTime = -1f;     // 장전 완료 시점 
    private bool _isReloading = false;
    private bool _isIdleReload = false;     //유휴 재장전 여부 
    private float _lastFireTime = -1f;      //마지막으로 발사한 시간
    private float _idleReloadDelay;


    public bool IsReady => Time.time >= _nextFireTime;
    public bool NeedsReload => CurrentAmmo < Data.maxAmmo;
    public bool IsReloading => _isReloading;

    public event Action<int> OnAmmoChanged;
    public event Action OnReloadStart;
    public event Action OnReloadComplete;

    public WeaponInstance(SO_WeaponBase data)
    {
        Data = data;
        CurrentAmmo = data.maxAmmo;
        _idleReloadDelay = data.idleReloadDelay;
        
    }

    public bool TryConsume()
    {
        if (!IsReady) return false;
        if (CurrentAmmo <= 0) return false;

        if(_isReloading && _isIdleReload)
        {
            _isReloading = false;
            _isIdleReload = false;
        }

        if(_isReloading) return false;

        CurrentAmmo--;
        _lastFireTime = Time.time;          //발사 시간 갱신
        OnAmmoChanged?.Invoke(CurrentAmmo);
        _nextFireTime = Time.time + Data.fireRate;

        if(CurrentAmmo <= 0)
        {
            StartReload(Data.reloadTime, isIdle: false);
        }

        return true;
    }

    private void StartReload(float duration, bool isIdle)
    {
        _isReloading = true;
        _isIdleReload = isIdle;
        _reloadEndTime = Time.time + Data.reloadTime;
        _lastFireTime = -1f;
        OnReloadStart?.Invoke();
    }

    //PlayerAttack의 Update에서 매 프레임 호출
    public void Tick()
    {
        if(_isReloading)
        {
            if(Time.time >= _reloadEndTime)
            {
                _isReloading = false;
                _isIdleReload = false;
                CurrentAmmo = Data.maxAmmo;
                OnAmmoChanged?.Invoke(CurrentAmmo);
                OnReloadComplete?.Invoke();
            }
            return;
        }

        //탄약이 1 ~ 2발 남아있고, 마지막 발사 후 IdleReloadDelay 경과 시 자동 장전
        if(CurrentAmmo > 0 && NeedsReload && _lastFireTime > 0f)
        {
            if(Time.time - _lastFireTime >= _idleReloadDelay)
                StartReload(Data.reloadTime, isIdle: true);
        }
    }

    //강제 즉시 장전 (외부 호출용 - 지금은 안 씀)
    public void Reload()
    {
        if (CurrentAmmo == Data.maxAmmo) return;
        _isReloading = false;
        _isIdleReload = false;
        _lastFireTime = -1f;
        CurrentAmmo = Data.maxAmmo;
        OnAmmoChanged?.Invoke(CurrentAmmo);
    }
}
