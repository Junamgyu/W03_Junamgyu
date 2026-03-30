using UnityEngine;

public class Level01_BossRotation : MonoBehaviour
{
    [SerializeField] private Transform _swordPivot;
    [SerializeField] private float _rotationSpeedPhase1 = 60f;
    [SerializeField] private float _rotationSpeedPhase2 = 120f;

    public float CurrentSpeed { get; private set;}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        CurrentSpeed = _rotationSpeedPhase1;        
    }

    // Update is called once per frame
    private void Update()
    {
        if(_swordPivot != null)
            _swordPivot.Rotate(0f, 0f, -CurrentSpeed * Time.deltaTime);
    }

    public void SetPhase2()
    {
        CurrentSpeed = _rotationSpeedPhase2;
    }

    public void SetSpeed(float speed)
    {
        CurrentSpeed = speed;
    }

    public void MultiplySpeed(float multiplier)
    {
        CurrentSpeed *= multiplier;
    }

    public float Phase1Speed => _rotationSpeedPhase1;
    public float Phase2Speed => _rotationSpeedPhase2;


}
