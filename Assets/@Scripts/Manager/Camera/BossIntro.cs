using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class BossIntro : MonoBehaviour
{
    public static event Action OnEndIntro;

    [SerializeField] private float _endTime = 4f;
    [SerializeField] private CinemachineCamera _main;
    [SerializeField] private CinemachineCamera _intro;

    

    private void OnEnable()
    {
        CameraManager.OnBossIntro += RunIntro;
    }

    private void OnDisable()
    {
        CameraManager.OnBossIntro -= RunIntro;
    }

    private void Start()
    {
        
    }

    public void RunIntro()
    {
        StartCoroutine(EndIntro());
    }

    IEnumerator EndIntro()
    {
        yield return new WaitForSeconds(_endTime);
        _main.Priority = 10;
        _intro.Priority = 5;
        yield return new WaitForSeconds(2);
        OnEndIntro?.Invoke();

    }
}
