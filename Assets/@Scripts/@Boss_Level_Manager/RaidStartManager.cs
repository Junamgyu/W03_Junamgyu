using System;
using System.Collections;
using UnityEngine;

public class RaidStartManager : MonoBehaviour
{
    public static RaidStartManager Instance {get; private set;}

    public float BossEncounterTime {get; private set;} //보스 조우 시작 시간
    public float ClearTime {get; private set;}      //클리어 까지 걸린 시간
    public float TotalSlowTime {get; private set;}  //슬로우 모션 총 사용 시간
    public int DeathCount => BossRespawnManager.Instance?.DeathCount ?? 0;

    private bool _isTracking = false;
    private bool _isSlowActive = false;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartTracking()
    {
        BossEncounterTime = Time.time;
        ClearTime = 0f;
        TotalSlowTime = 0f;
        _isTracking = true;
        StartCoroutine(TrackingRoutine());
        Debug.Log("레이드 시작");
    }

    public void StopTracking()
    {
        if(!_isTracking) return;
        _isTracking = false;
        ClearTime = Time.time - BossEncounterTime;
    }

    public void OnSlowStart()
    {
        _isSlowActive = true;
    }

    public void OnSlowEnd()
    {
        _isSlowActive = false;
    }

    IEnumerator TrackingRoutine()
    {
        while(_isTracking)
        {
            if(_isSlowActive)
                TotalSlowTime += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    public String FormatTime(float seconds)
    {
        int min = (int)(seconds / 60);
        int sec = (int)(seconds % 60);
        return $"{min:00}:{sec:00}";
    }

    public void ResetStats()
    {
        ClearTime = 0f;
        TotalSlowTime = 0f;
        _isTracking = false;
        StopAllCoroutines();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
