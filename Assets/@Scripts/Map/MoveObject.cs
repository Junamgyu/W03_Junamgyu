using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public enum MoveType { PingPong, Loop, OneWay}
    [Header("이동 경로")]
    [Tooltip("오브젝트가 거쳐갈 웨이포인트 목록 (로컬 좌표 기준)")]
    public Vector2 [] wayPoints;

    [Header("이동 설정")]
    public float moveSpeed = 3f;
    public MoveType moveType = MoveType.PingPong;

    [Header("대기 시간")]
    [Tooltip("각 웨이포인트 도착 후 대기 시간 (초)")]
    public float waitTime = 0f;

    [Header("래버")]
    public bool isWarking = false;

    // -- 내부 변수
    private Vector2 [] _worldPoints;
    private int _targetIndex = 1;
    private int _direction = 1;
    private float _waitTImer= 0f;
    private bool _waiting = false;

    private Vector2 _previousPosition;          //이전 프레임 위치
    private Rigidbody2D  _passengerRb;          // 현재 탑승 중인 플레이어
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(wayPoints == null || wayPoints.Length < 2)
        {
            Debug.LogWarning($"[MoveObject] {name}: wayPoints를 2개 이상 설정해야함");
            enabled = false;
            return;
        } 

        _worldPoints = new Vector2[wayPoints.Length];
        for(int i = 0; i < wayPoints.Length; i++)
        {
            _worldPoints[i] = (Vector2)transform.position + wayPoints[i];
        }

        transform.position = _worldPoints[0];
        _previousPosition = _worldPoints[0];
    }

    // Update is called once per frame
    void Update()
    {
        if(isWarking)
        {
                if(_waiting)
            {
                _waitTImer -= Time.deltaTime;
                if(_waitTImer <= 0) _waiting = false;
                return;
            }
            MoveToTarget();
        }
        
    }

    void LateUpdate()
    {
        //이전 프레임에 플랫폼이 얼마나 움직였는지 계산
        Vector2 delta = (Vector2)transform.position - _previousPosition;
        

        //탑승 중인 플레이어에게 델타만큼 이동 전달
        if(_passengerRb != null && delta != Vector2.zero)
        {
            _passengerRb.transform.position = (Vector2)_passengerRb.transform.position + delta;
            Debug.Log($"[MoveObject] delta: {delta}, passengerRb 위치: {_passengerRb.position}");
            _passengerRb.MovePosition(_passengerRb.position + delta);
            Debug.Log($"[MoveObject] MovePosition 호출 완료");
        }

        _previousPosition = transform.position;
    }


    void MoveToTarget()
    {
        Vector2 target = _worldPoints[_targetIndex];
        Vector2 current = transform.position;

        transform.position = Vector2.MoveTowards(current, target, moveSpeed * Time.deltaTime);

        if(Vector2.Distance(transform.position, target) < 0.01f)
        {
            transform.position = target;

            if(waitTime > 0f)
            {
                _waiting = true;
                _waitTImer = waitTime;
            }

            AdvanceIndex();
        }
    }

    void AdvanceIndex()
    {
        switch(moveType)
        {
            case MoveType.PingPong:
                _targetIndex += _direction;
                if(_targetIndex >= _worldPoints.Length || _targetIndex < 0)
                {
                    _direction *= -1;
                    _targetIndex += _direction * 2;
                }
                break;

            case MoveType.Loop:
                _targetIndex = (_targetIndex + 1) % _worldPoints.Length;
                break;
            
            case MoveType.OneWay:
                if(_targetIndex < _worldPoints.Length - 1) _targetIndex ++;
                break;
            
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if(!collision.gameObject.CompareTag("Player")) return;

        float normalY = collision.contacts[0].normal.y;
        Debug.Log($"[MoveObject] 충돌 법선 Y값: {normalY}");

        //플랫폼 윗면에서 충돌했을 때만 (옆면, 아랫면 제외)
        if(normalY < -0.5f)
        {
            _passengerRb = collision.gameObject.GetComponent<Rigidbody2D>();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(!collision.gameObject.CompareTag("Player")) return;

        _passengerRb = null;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(wayPoints == null || wayPoints.Length < 2) return;

        Vector2 origin = Application.isPlaying ? _worldPoints[0] : (Vector2)transform.position;

        for(int i = 0; i < wayPoints.Length; i++)
        {
            Vector2 worldPt = Application.isPlaying ? _worldPoints[i] : origin + wayPoints[i];

            Gizmos.color = (i == 0) ? Color.green : Color.yellow;
            Gizmos.DrawSphere(worldPt, 0.15f);

            if(i < wayPoints.Length - 1)
            {
                Vector2 next = Application.isPlaying ? _worldPoints[i + 1] : origin + wayPoints[i + 1];
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(worldPt, next);
            }
        }
    }
#endif
}
