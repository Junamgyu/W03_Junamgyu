using System.Collections;
using DG.Tweening;
using NUnit.Framework;
using UnityEngine;

public class Level01_BossOrbitSword : MonoBehaviour
{
    public enum SwordState {Orbit, Free, Dead}

    public bool IsInPattern {get; set;} = false;
    #region 인스펙터 변수
    [Header("체력")]
    [SerializeField] private int _maxHp = 3;

    [Header("데미지")]
    [SerializeField] private int _contactDamage = 1;
    [SerializeField] private float _damageCooldown = 0.5f;

    [Header("Free 상태 - 떠다니기")]
    [SerializeField] private float _floatRadius = 2.5f;     //보스 주변 떠다니는 반경
    [SerializeField] private float _floatSpeed = 1.5f;      // 공전 속도
    [SerializeField] private float _floatAmplitude = 0.5f;  //위아래 흔들림 폭

    [Header("Free 상태 - 공격")]
    [SerializeField] private float _attackSpeed = 10f;      //플레이어 향해 날아가는 속도
    [SerializeField] private float _returnSpeed = 5f;       //공격 후 돌아오는 속도
    [SerializeField] private float _attackReturenDelay = 0.5f;  //공격 후 복귀 딜레이
    [SerializeField] private int _attackWarnFlashCount = 3;
    [SerializeField] private float _attackWarnFlashInterval = 0.1f;
    [SerializeField] private Color _attackWarnColor = Color.red;

    [Header("피격 연출")]
    [SerializeField] private int _hitFlashCount = 2;
    [SerializeField] private float _hitFlashInterval = 0.06f;
    
    #endregion

    #region 내부 변수
    public SwordState State {get; private set;} = SwordState.Orbit;
    public bool IsOrbit => State == SwordState.Orbit;

    private int _currentHp;
    private float _lastDamageTime = -999f;
    private Transform _boss;
    private Transform _player;
    private SpriteRenderer _sr;
    private Coroutine _flashCoroutine;
    private Coroutine _behaviorCoroutine;

    //Free 상태 공전 각도
    private float _floatAngle = 0f;
    private Vector3 _freeBasePos;
    private bool _isAttacking = false;
    
    #endregion

    #region 초기화

    public void Initialize(Transform boss, Transform player)
    {
        _boss = boss;
        _player = player;
        _currentHp = _maxHp;
        State = SwordState.Orbit;
    }

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }
    #endregion

    #region 피격
    private void OnTriggerEnter2D(Collider2D other)
    {
        //플레이어 총알에 맞았을 때
        if(other.gameObject.layer == LayerMask.NameToLayer("ShotgunBullet")
            || other.CompareTag("ShotgunBullet"))
        {
            TakeDamage(1);
            return;
        }

        //플레이어 접촉 데미지 (Free 상태에서만)
        if(State == SwordState.Free && other.CompareTag("Player"))
        {
            if(Time.time - _lastDamageTime < _damageCooldown) return;
            other.GetComponent<IDamageable>()?.TakeDamage(_contactDamage);
            _lastDamageTime = Time.time;
        }

        //Orbit 상태 접촉 데미지
        if(State == SwordState.Orbit && other.CompareTag("Player"))
        {
            if(Time.time - _lastDamageTime < _damageCooldown) return;
            other.GetComponent<IDamageable>()?.TakeDamage(_contactDamage);
            _lastDamageTime = Time.time;
        }
    }

    public void TakeDamage(int damage)
    {
    
        if(State == SwordState.Dead) return;
        if(State == SwordState.Free) return;
        if(IsInPattern) return;
        
        _currentHp -= damage;

        if(_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(HitFlashRoutine());

        if(_currentHp <= 0)
            TransitionToFree();
    }

    IEnumerator HitFlashRoutine()
    {
        if (_sr == null) yield break;
        Color original = _sr.color;
        for(int i = 0; i < _hitFlashCount; i++)
        {
            _sr.color = Color.white;
            yield return new WaitForSeconds(_hitFlashInterval);
            _sr.color = original;
            yield return new WaitForSeconds(_hitFlashInterval);
        }
    }

    #endregion

    #region 상태 전환
    //orbit -> Free
    void TransitionToFree()
    {
        if(_boss == null)
        {
            gameObject.SetActive(false);
            return;
        }

        State = SwordState.Free;

        //피봇에서 분리
        transform.SetParent(null);

        //보스 주변 현재 위치 기준으로 공전 시작
        _floatAngle = Mathf.Atan2(transform.position.y - _boss.position.y,
                        transform.position.x - _boss.position.x) *Mathf.Rad2Deg;

        if(_behaviorCoroutine != null) StopCoroutine(_behaviorCoroutine);
            _behaviorCoroutine = StartCoroutine(FreeFloatRoutine());

        //보스에게 칼 상태 변경 알림
        _boss.GetComponent<Level01_Boss>()?.OnSwordDestroyed(this);
    }

    public void DestoryFree()
    {
        if(State == SwordState.Dead) return;
        State = SwordState.Dead;

        if(_behaviorCoroutine != null) StopCoroutine(_behaviorCoroutine);

        //사라지는 연출
        transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
            .OnComplete(() => gameObject.SetActive(false));
    }

    #endregion

    #region Free 상태 - 보스 주변 떠다니기
    
    IEnumerator FreeFloatRoutine()
    {
        int index = _boss.GetComponent<Level01_Boss>()?.GetSwordIndex(transform) ?? 0;

        while(State == SwordState.Free && !_isAttacking)
        {
            if(_boss == null) yield break;

            //목표 위치로 부드럽게 정렬 (보스 X 축 나란히)
            float xOffset = (index - 2.5f) * _floatRadius * 0.6f;
            float yOffset = 10f;

            Vector3 target = _boss.position + new Vector3(xOffset, yOffset, 0f);

            transform.position = Vector3.Lerp(
                transform.position, target, Time.deltaTime * 5f
            );

            transform.rotation = Quaternion.Euler(0f, 0f, 180f);


            yield return null;
        }
    }
    #endregion

    #region  Free 상태 - 패턴 공격

    //보스가 패턴 실행할 때 호출
    public void LaunchAttack()
    {
        if(State != SwordState.Free) return;
        if(_isAttacking) return;
        if(_player == null) return;

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        if(_behaviorCoroutine != null) StopCoroutine(_behaviorCoroutine);

        //플레이어 방향으로 발사
        Vector3 startPos = transform.position;
        Vector2 dir = (_player.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        float maxDistance = 15f;
        float traveled = 0f;

        while(traveled < maxDistance)
        {
            if(this == null) yield break;

            float step = _attackSpeed * Time.deltaTime;
            transform.position += (Vector3)(dir * step);
            traveled += step;

            yield return null;
        }

        //딜레이 후 복귀
        yield return new WaitForSeconds(_attackReturenDelay);

        // 보스 위치로 복귀
        while(State == SwordState.Free)
        {
            if(_boss == null) yield break;

            Vector2 toOwner = (_boss.position - transform.position).normalized;
            transform.position += (Vector3)(toOwner * _returnSpeed * Time.deltaTime);

            if(Vector3.Distance(transform.position, _boss.position) < _floatRadius + 0.5f)
                break;
            yield return null;
        }

        _isAttacking = false;

        if(State == SwordState.Free)
            _behaviorCoroutine = StartCoroutine(FreeFloatRoutine());
    }

    public void LaunchStraightAttack()
    {
        if(State != SwordState.Free) return;
        if(_isAttacking) return;
        if(_player == null) return;

        StartCoroutine(StraightAttackRoutine());
    }

    IEnumerator StraightAttackRoutine()
    {
        _isAttacking = true;
        if(_behaviorCoroutine != null) StopCoroutine(_behaviorCoroutine);

        if(_sr != null)
        {
            Color originalColor = _sr.color;
            for(int i = 0; i < _attackWarnFlashCount; i++)
            {
                _sr.color = _attackWarnColor;
                yield return new WaitForSeconds(_attackWarnFlashInterval);
                _sr.color = originalColor;
                yield return new WaitForSeconds(_attackWarnFlashInterval);
            }
        }

        //발사 지점 플레이어 위치 고정
        Vector3 targetPos = _player.position;
        Vector2 dir = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        while(true)
        {
            if(this == null) yield break;

            transform.position += (Vector3)(dir * _attackSpeed * Time.deltaTime);

            Vector3 toTarget = targetPos - transform.position;
            if(Vector3.Dot(toTarget, (Vector3)dir) <= 0f)
                break;

            yield return null;
        }

        transform.position = targetPos;

        yield return new WaitForSeconds(_attackReturenDelay);

        while(State == SwordState.Free)
        {
            if(_boss == null) yield break;

            Vector2 toOwner = (_boss.position - transform.position).normalized;
            transform.position += (Vector3)(toOwner * _returnSpeed * Time.deltaTime);

            if(Vector3.Distance(transform.position, _boss.position) < _floatRadius + 0.5f)
                break;

            yield return null;
        }

        _isAttacking = false;

        if(State == SwordState.Free)
            _behaviorCoroutine = StartCoroutine(FreeFloatRoutine());
    }


    #endregion

    public void ResetSword()
    {
        transform.DOKill(true);
        StopAllCoroutines();
        _behaviorCoroutine = null;
        _flashCoroutine = null;

        State = SwordState.Orbit;
        _currentHp = _maxHp;
        _isAttacking = false;
        _lastDamageTime = -999f;
        IsInPattern = false;        

        //스프라이트 원래 색으로
        if(_sr != null) _sr.color = Color.white;
    }
}
