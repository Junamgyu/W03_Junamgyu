using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public float _moveSpeed = 5f;
    public float _shotgunForce = 20f;
    public float _gravityOffDuration = 0.5f;
    public float _damplingDuration = 0.1f;
    public float _damplingValue = 8f;
    public float _shootXMul = 1.5f;

    public bool _isMove = false;
    public float _originalGravity;

}
