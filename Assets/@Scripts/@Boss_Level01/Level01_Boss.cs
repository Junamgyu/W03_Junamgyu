using System.Collections;
using DG.Tweening;
using NUnit.Framework.Constraints;
using UnityEngine;

public class Level01_Boss : EnemyBase
{
    #region 인스펙터 변수
    public bool IsPhase2 => _isPhase2;

    [Header("컴포넌트")]
    [SerializeField] private SpriteRenderer _bodyRenderer;
    [SerializeField] private Transform _swordPivot;
    [SerializeField] private Transform[] _swords;

    [Header("회전 설정")]
    [SerializeField] private float _rotationSpeedPhase1 = 60f;
    [SerializeField] private float _rotationSpeedPhase2 = 120f;

    [Header("패턴 공통")]
    [SerializeField] private float _idleDuration = 1.5f;
    [SerializeField] private float _tellDuration = 0.6f;
    [SerializeField] private float _punishDuration = 0.5f;
    [SerializeField] private float _returnSpeed = 6f;
    [SerializeField] private float _freeSwordLaunchInterval = 0.3f;


    [Header("패턴 1 - 칼 던지기")]
    [SerializeField] private float _throwSpeed = 20f;
    [SerializeField] private float _throwReturnDelay = 0.2f;
    [SerializeField] private float _throwReturnSpeed = 22f;     //돌아오는 속도 
    [SerializeField] private float _overshootDistance = 3f;     //플레이어 지나치는 거리

    [Header("패턴 2 - 내려찍기")]
    [SerializeField] private float _slamRiseSpeed = 8f;
    [SerializeField] private float _slamSpeed = 18f;
    [SerializeField] private float _slamHeight = 4f;
    [SerializeField] private GameObject _shockwavePrefab;
    [SerializeField] private LayerMask _groundLayer;

    [Header("패턴 3 - 칼 콤보")]
    [SerializeField] private float _comboMoveSpeed = 12f;
    [SerializeField] private float _comboCooldown = 0.25f;
    [SerializeField] private float _comboApproachDistance = 2f;  // 접근 거리
    [SerializeField] private float _comboSwingDuration = 0.15f;  // 스윙 속도
    [SerializeField] private float _comboSwingRange = 80f;       // 휘두르는 각도 범위
    [SerializeField] private float _comboRadius = 1.5f;          // 칼 길이

    [SerializeField] private Vector2 _comboTargetOffset = Vector2.zero;
    [SerializeField] private GameObject _slashHitboxPrefab;

    [Header("패턴 4 - 칼 낙하")]
    [SerializeField] private GameObject _dropSwordPrefab;   // 전용 낙하 칼 프리팹
    [SerializeField] private float _dropSpeed = 20f;
    [SerializeField] private float _dropSpeedPhase2 = 35f;      //? transition Drop Speed of Phase2
    [SerializeField] private int _dropCountMin = 4;         // 최소 횟수
    [SerializeField] private int _dropCountMax = 7;         // 최대 횟수\
    [SerializeField] private int _dropCountPhase2Bonus = 3;     //? Add to count of Phase2
    [SerializeField] private float _dropRange = 4f;
    [SerializeField] private float _dropInterval = 0.3f;
    [SerializeField] private float _dropHeight = 8f;
    [SerializeField] private float _dropStickDuration = 0.8f; // 박힌 후 유지 시간
    [SerializeField] private float _dropWarningDelay = 1.5f;  // 추가 — 스폰 후 대기 시간
    [SerializeField] private float _dropSwordPivotOffset = 0.5f; // 추가 — 칼 피벗 오프셋
    [SerializeField] private LayerMask _dropGroundLayer;    // 낙하 바닥 감지용

    [Header("패턴 5 - 전방위 칼 발사")]
   [SerializeField] private float _burstRushSpeed = 15f;      // 돌진 속도
    [SerializeField] private float _burstRushDistance = 3f;    // 돌진 거리
    [SerializeField] private float _burstSwordSpeed = 12f;     // 칼 발사 속도
    [SerializeField] private float _burstInterval = 0.5f;      // 발사 간격
    [SerializeField] private float _burstSwordLifetime = 3f;   // 칼 소멸 시간
    [SerializeField] private Vector2 _burstTargetOffset = Vector2.zero;
    [SerializeField] private GameObject _burstSwordPrefab;     // 발사용 칼 프리팹

    [Header("피격 연출")]
    [SerializeField] private int _hitFlashCount = 3;
    [SerializeField] private float _hitFlashInterval = 0.08f;

    #endregion

    #region 색깔 상태
    [SerializeField] private Color _colorIdle = Color.red;
    private readonly Color _colorTell = Color.red;
    private readonly Color _colorPunish = Color.yellow;
    private readonly Color _colorImmune = new Color(0.3f, 0.3f, 0.3f);

    #endregion

    #region 내부 변수

    private Transform _player;
    private Vector3 _originPos;
    private bool _isPhase2 = false;
    private bool _isImmune = false;
    private float _currentRotationSpeed;
    private Coroutine _flashCoroutine;
    private Coroutine _patternCoroutine;
    private Level01_BossHealth _bossHealth;
    private Level01_BossRotation _bossRotation;
    private bool _isStunned = false;

    // 칼 원래 로컬 위치 저장
    private Vector3[] _swordOriginalLocalPos;
    private Vector3[] _swordOriginalLocalScale;
    private Quaternion[] _swordOriginalLocalRot;


    #endregion

    #region 생명주기

    protected override void Start()
    {
        base.Start();

        RaidStartManager.Instance?.StartTracking();     //! 보스 조우 시 트래킹 시작

        _originPos = transform.position;
        _currentRotationSpeed = _rotationSpeedPhase1;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if(playerObj != null) _player = playerObj.transform;

        //칼 원래 위치 저장
        _swordOriginalLocalPos = new Vector3[_swords.Length];
        _swordOriginalLocalScale = new Vector3[_swords.Length];
        _swordOriginalLocalRot = new Quaternion[_swords.Length];
        for(int i = 0; i < _swords.Length; i++)
        {
            if(_swords[i] == null) continue;
            _swordOriginalLocalPos[i] = _swords[i].localPosition;
            _swordOriginalLocalScale[i] = _swords[i].localScale;
            _swordOriginalLocalRot[i] = _swords[i].localRotation;

            // 칼에 보스 플레이어 참조 전달
            var orbitSword = _swords[i].GetComponent<Level01_BossOrbitSword>();
            if(orbitSword != null)
                orbitSword.Initialize(transform, _player);
        }

        SetBodyColor(_colorIdle);
        _patternCoroutine = StartCoroutine(PatternCycleRoutine());

        _bossHealth = GetComponent<Level01_BossHealth>();
        _bossRotation = GetComponent<Level01_BossRotation>();                
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Update is called once per frame
    void Update()
    {
        _swordPivot.position = transform.position;

        _swordPivot.Rotate(0f, 0f, -_currentRotationSpeed * Time.deltaTime);

    
     if (_isPatternRunning) return;

        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard == null) return; 

        if (keyboard.digit1Key.wasPressedThisFrame)
            StartCoroutine(RunPattern(Pattern1_ThrowSword()));
        else if (keyboard.digit2Key.wasPressedThisFrame)
            StartCoroutine(RunPattern(Pattern2_Slam()));
        else if (keyboard.digit3Key.wasPressedThisFrame)
            StartCoroutine(RunPattern(Pattern3_SwordCombo()));
        else if (keyboard.digit4Key.wasPressedThisFrame)
            StartCoroutine(RunPattern(Pattern4_DropSword()));
        else if (keyboard.digit5Key.wasPressedThisFrame)
            StartCoroutine(RunPattern(Pattern5_BurstSword()));
        else if (keyboard.digit6Key.wasPressedThisFrame)
        {
            _currentHp = Mathf.FloorToInt(_maxHp * 0.49f);
            Debug.Log($"디버그 - 보스 체력 강제 설정 : {_currentHp} / {_maxHp}");
            if (!_isPhase2)
                StartCoroutine(EnterPhase2Routine());            
        }
    }

    public void OnSwordDestroyed(Level01_BossOrbitSword sword)
    {
        if(_isStunned) return;

        bool anyOrbit = false;
        foreach (var s in _swords)
        {
            if(s == null) continue;
            var orbitSword = s.GetComponent<Level01_BossOrbitSword>();
            if(orbitSword != null && orbitSword.IsOrbit)
            {
                anyOrbit = true;
                break;
            }
        }
        if(!anyOrbit) StartCoroutine(StunRoutine());
    }
    #endregion

    #region 보스 스턴
    IEnumerator StunRoutine()
    {
        //무력화 시작
        _isImmune = false;  //데미지 받기 시작
        _isStunned = true;
        SetBodyColor(Color.cyan);       // 무력화 색깔

        if(_patternCoroutine != null)
            StopCoroutine(_patternCoroutine);
        
        yield return new WaitForSeconds(4.5f);

        _isStunned = false;
        SetBodyColor(_colorIdle);
        RegenerateOrbitSwords();

        _patternCoroutine = StartCoroutine(PatternCycleRoutine());
    }

    IEnumerator RunPattern(IEnumerator pattern)
    {
        _isPatternRunning = true;
        yield return StartCoroutine(pattern);

        // 공통 후딜
        SetBodyColor(_colorPunish);
        _isImmune = false;
        yield return new WaitForSeconds(_punishDuration);

        SetBodyColor(_colorIdle);
        _isPatternRunning = false;
    }

    void RegenerateOrbitSwords()
    {
        for (int i = 0; i < _swords.Length; i++)
        {
            if (_swords[i] == null) continue;

            var orbitSword = _swords[i].GetComponent<Level01_BossOrbitSword>();
            if (orbitSword == null) continue;

            // DOTween 트윈 전부 강제 종료
            _swords[i].DOKill(true);

            // 칼 상태 초기화
            orbitSword.ResetSword();

            _swords[i].gameObject.SetActive(true);  //오브젝트 활성화

            // 피봇에 다시 붙이기
            _swords[i].SetParent(_swordPivot);

            // 원래 위치/회전 복구
            _swords[i].localPosition = _swordOriginalLocalPos[i];
            _swords[i].localRotation = _swordOriginalLocalRot[i];


            _swords[i].localScale = Vector3.zero;
            _swords[i].DOScale(_swordOriginalLocalScale[i], 0.3f).SetEase(Ease.OutBack).SetDelay(i * 0.2f);

            
        }
    }

    #endregion

    #region 피격 / 사망
    public override void TakeDamage(int damage, bool isAddGauge = false)
    {
        // 칼이 Orbit 상태로 남아 있으면 무적
        if(HasOrbitSword()) return;
        if(_isImmune) return;

        if(_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(HitFlashRoutine());

        _bossHealth.OnHit();
        base.TakeDamage(damage, isAddGauge);

        if(!_isPhase2 && _currentHp <= _maxHp * 0.5f)
            StartCoroutine(EnterPhase2Routine());
    }

    bool HasOrbitSword()
    {
        foreach(var s in _swords)
        {
            if(s == null) continue;
            var orbitSword = s.GetComponent<Level01_BossOrbitSword>();
            if(orbitSword != null && orbitSword.IsOrbit) return true;
        }
        return false;
    }

    public override void Die()
    {
        StopAllCoroutines();
        if(_bossHealth != null) _bossHealth.OnBossDie();
        StartCoroutine(BossDieRoutine());
    }

    //죽었을 경우
    IEnumerator BossDieRoutine()
    {
        RaidStartManager.Instance?.StopTracking();      //! 보스 사망 시 트래킹 종료

        _isImmune = true;
        _currentRotationSpeed = 0f;

        //칼들이 사방으로 튕겨 나가는 연출
        foreach (var sword in _swords)
        {
            if(sword == null) continue;
            sword.SetParent(null);
            Vector2 dir = (sword.position - transform.position).normalized;
            sword.DOMove(sword.position + (Vector3)(dir * 5f), 0.5f).SetEase(Ease.OutQuad);
            sword.DORotate(new Vector3(0f, 0f, Random.Range(0f, 360f)), 0.5f);
        }

        yield return new WaitForSeconds(0.3f);

        transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(0.5f);

        foreach(var sword in _swords)
            if(sword != null) Destroy(sword.gameObject);

        gameObject.SetActive(false);

        if(ManagerRegistry.TryGet<GameStateManager>(out var gsm))
            gsm.ChangeState(GameState.Clear);
    }

    IEnumerator HitFlashRoutine()
    {
        Color restoreColor;
        if(_isStunned) 
            restoreColor = Color.cyan;
        else if (_isImmune)
            restoreColor = _colorImmune;
        else
            restoreColor = _colorIdle;
    
        for(int i = 0; i < _hitFlashCount; i++)
        {
            _bodyRenderer.color = Color.white;
            yield return new WaitForSeconds(_hitFlashInterval);
            _bodyRenderer.color = restoreColor;
            yield return new WaitForSeconds(_hitFlashInterval);
        }
    }
    #endregion

    #region 페이즈 관리
    
    IEnumerator EnterPhase2Routine()
    {
        _isPhase2 = true;
        _isImmune = true;

        if(_patternCoroutine != null)
            StopCoroutine(_patternCoroutine);
        
        //페이즈 전환 연출
        for (int i = 0; i < 8; i++)
        {
            SetBodyColor(Color.white);
            _currentRotationSpeed = _rotationSpeedPhase2 * 2f;
            yield return new WaitForSeconds(0.1f);
            SetBodyColor(Color.black);
            _currentRotationSpeed = 0f;
            yield return new WaitForSeconds(0.1f);
        }

        _currentRotationSpeed = _rotationSpeedPhase2;
        _isImmune = false;
        SetBodyColor(_colorIdle);

        if(_bossHealth != null) _bossHealth.ForceUpdateHpUI();
        _patternCoroutine = StartCoroutine(PatternCycleRoutine());
    }
    #endregion

    IEnumerator PatternCycleRoutine()
    {
        // 디버그용 — 키보드로 패턴 수동 실행
        while (true)
        {
            yield return null;
        }
    }
    private bool _isPatternRunning = false;


    /* //! 원본 랜덤 패턴 사이클 (디버그 후 복구 할 예정임)------------------
    #region 패턴 사이클 원본
    IEnumerator PatternCycleRoutine()
    {
        while(true)
        {
            SetBodyColor(_colorIdle);
            _isImmune = false;
            yield return new WaitForSeconds(_idleDuration);

            //1 페이즈 : 0 ~ 2, 2페이즈 0 ~ 4
            int maxPattern = _isPhase2 ? 5 : 3;
            int pattern = Random.Range(0, maxPattern);

            switch(pattern)
            {
                case 0: yield return StartCoroutine(Pattern1_ThrowSword()); break;
                case 1: yield return StartCoroutine(Pattern2_Slam()); break;
                case 2: yield return StartCoroutine(Pattern3_SwordCombo()); break;
                case 3: yield return StartCoroutine(Pattern4_DropSword()); break;
                case 4: yield return StartCoroutine(Pattern5_GroundSword()); break;
            }

            // 공통 후딜 - 플레이어 공격 타이밍
            SetBodyColor(_colorPunish);
            _isImmune = false;
            yield return new WaitForSeconds(_punishDuration);
        }
    }

    #endregion */ //!--------------------------------------------

    #region 패턴 1 - 칼 던지기 (가드 가능)
    
    IEnumerator Pattern1_ThrowSword()
    {
        if(_player == null) yield break;

        Debug.Log("보스 패턴 1번 실행");    

        yield return StartCoroutine(TellRoutine());

        if(_isPhase2)
        {
            Transform sword1 = GetAvailableSword();
            Transform sword2 = sword1 != null ? GetNextAvailableSword(sword1) : null;

            if(sword1 != null) StartCoroutine(ThrowSwordRoutine(sword1));
            if(sword2 != null) StartCoroutine(ThrowSwordRoutine(sword2));

            yield return new WaitForSeconds(3f);
        }
        else
        {
            Transform sword = GetAvailableSword();
            if(sword != null) yield return StartCoroutine(ThrowSwordRoutine(sword)); 
        }
        StartCoroutine(LaunchFreeSwordsSequential());
    }


    //? Trow the Sword logic
    IEnumerator ThrowSwordRoutine(Transform sword)
    {

        var orbitSword = sword.GetComponent<Level01_BossOrbitSword>();
        int sowrdIndex = GetSwordIndex(sword);                              //칼을 피봇에서 분리
        
        if(orbitSword != null) orbitSword.IsInPattern = true;               //패턴 시작
        sword.SetParent(null);

        Vector2 dir = (_player.position - sword.position).normalized;
        
        //플레이어 지나친 목표 지점
        Vector3 targetPos = _player.position + (Vector3)(dir * _overshootDistance);
        //날아가기 - 플레이어 위치까지 도달하거나 최대 거리 초과 시 멈춤
        while(true)
        {
            if(sword == null) yield break;
            sword.position += (Vector3)(dir * _throwSpeed * Time.deltaTime);
            sword.Rotate(0f, 0f, -720f * Time.deltaTime);

            if(Vector3.Dot(targetPos - sword.position, dir) <= 0f) break;
            yield return null;
        }

        //목표 지점에 정확한 스냅
        sword.position = targetPos;

        //잠깐 멈추는 딜레이
        yield return new WaitForSeconds(_throwReturnDelay);

        //돌아오기
        while(true)
        {
            if(sword == null) break;

            Vector2 toOwner = (transform.position - sword.position).normalized;
            sword.position += (Vector3)(toOwner * _throwReturnSpeed * Time.deltaTime);
            sword.Rotate(0f, 0f, -720f * Time.deltaTime);

            if(Vector3.Distance(sword.position, transform.position) < 0.5f) break;
            
            yield return null;
        }

        //피봇 복귀
        if(sword != null && sowrdIndex >= 0)
        {
            if(orbitSword != null) orbitSword.IsInPattern = false;
            sword.SetParent(_swordPivot);
            sword.localPosition = _swordOriginalLocalPos[sowrdIndex];
            sword.localRotation = _swordOriginalLocalRot[sowrdIndex];
        }
    }

    Transform GetNextAvailableSword(Transform exclude)
    {
        foreach(var sword in _swords)
        {
            if(sword == null) continue;
            if(sword == exclude) continue;
            if(sword.parent != _swordPivot) continue;

            var orbitSword = sword.GetComponent<Level01_BossOrbitSword>();
            if(orbitSword != null && orbitSword.State != Level01_BossOrbitSword.SwordState.Orbit)
                continue;
            
            return sword;
        }
        return null;
    }
    #endregion

    #region 패턴 2 내려 찍기 (가드 불가 / 2페이즈 충격파 가드 가능)

    IEnumerator Pattern2_Slam()
    {
        if (_player == null) yield break;

        Debug.Log("보스 패턴 2번 실행");

        yield return StartCoroutine(TellRoutine());
        StartCoroutine(LaunchFreeSwordsSequential());       //패턴 2는 시작시 Free칼 공격

        int slamCount = _isPhase2 ? 2 : 1;      //? if phase2 than SlamCount 2

        for (int s = 0; s < slamCount; s++)
        {
                // 플레이어 위로 이동
            Vector3 riseTarget = new Vector3(_player.position.x, _player.position.y + _slamHeight, 0f);
            yield return StartCoroutine(MoveToPosition(riseTarget, _slamRiseSpeed));

            // 예고 멈춤
            SetBodyColor(Color.red);
            yield return new WaitForSeconds(0.4f);

            // 내려찍기
            float slamX = _player.position.x;
            while(true)
            {
                transform.position = Vector3.MoveTowards(transform.position, 
                    new Vector3(slamX, transform.position.y - 100f, 0f), 
                    _slamSpeed * Time.deltaTime);
                
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.3f, _groundLayer);
                if(hit.collider != null)
                {
                    transform.position = new Vector3(transform.position.x, hit.point.y, 0f);
                    break;
                }
                yield return null;
            }

            // 충격 연출
            transform.DOPunchScale(new Vector3(0.8f, -0.8f, 0f), 0.3f, 8, 0.8f);

            // 2페이즈 충격파
            if (_isPhase2 && _shockwavePrefab != null)
                Instantiate(_shockwavePrefab, transform.position, Quaternion.identity);

            yield return new WaitForSeconds(0.3f);

        }
        yield return StartCoroutine(MoveToPosition(_originPos, _returnSpeed));
    }
        

    #endregion

    #region 패턴 3 - 칼 3회 콤보 (가드 가능)

    IEnumerator Pattern3_SwordCombo()
    {
        if (_player == null) yield break;
        
        int comboCount = _isPhase2 ? 4 : 3;

    Debug.Log("보스 패턴 3번 실행");

    yield return StartCoroutine(TellRoutine());
    //StartCoroutine(LaunchFreeSwordsSequential());       //Free 상태 칼 추가공격

    Transform sword = GetAvailableSword();
    if (sword == null) yield break;

    var orbitSword = sword.GetComponent<Level01_BossOrbitSword>();
    int swordIndex = GetSwordIndex(sword);
    
    if(orbitSword != null) orbitSword.IsInPattern = true;
    sword.SetParent(null); // 피봇에서 분리

    float radius = _comboRadius;
    float swingRange = _comboSwingRange;
    float swingDuration = _comboSwingDuration;

    Vector3 targetPos = _player.position + new Vector3(_comboTargetOffset.x, _comboTargetOffset.y, 0f);

    // 첫 휘두름 방향 계산 — 플레이어 방향 기준
    Vector2 toPlayer = (targetPos - transform.position).normalized;
    float centerAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

    // 시작 각도 초기화 — 첫 번째는 왼쪽에서 시작
    float currentStart = centerAngle - swingRange;
    float currentEnd = centerAngle + swingRange;

    for (int i = 0; i < comboCount; i++)
    {
        targetPos = _player.position + new Vector3(_comboTargetOffset.x, _comboTargetOffset.y, 0f);

        // 플레이어 방향으로 접근
        Vector3 attackPos = Vector3.MoveTowards(
            transform.position, targetPos, _comboApproachDistance
        );
        yield return StartCoroutine(MoveToPosition(attackPos, _comboMoveSpeed));

        // 접근 후 플레이어 방향 재계산
        targetPos = _player.position + new Vector3(_comboTargetOffset.x, _comboTargetOffset.y, 0f);
        toPlayer = (targetPos - transform.position).normalized;
        centerAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

        // 진자 운동 — 이전 끝 각도가 다음 시작 각도
        // i==0: 왼→오, i==1: 오→왼, i==2: 왼→오
        if (i == 0)
        {
            currentStart = centerAngle - swingRange;
            currentEnd   = centerAngle + swingRange;
        }
        else
        {
            // 이전 끝 각도 ↔ 시작 각도 교체 (진자)
            float temp = currentStart;
            currentStart = currentEnd;
            currentEnd   = temp;
        }

        // 칼 시작 위치 배치
        float startRad = currentStart * Mathf.Deg2Rad;
        sword.position = transform.position + new Vector3(
            Mathf.Cos(startRad) * radius,
            Mathf.Sin(startRad) * radius,
            0f
        );
        sword.rotation = Quaternion.Euler(0f, 0f, currentStart);

        // 히트박스 생성
        GameObject hitbox = null;
        if (_slashHitboxPrefab != null)
            hitbox = Instantiate(_slashHitboxPrefab, sword.position, Quaternion.identity);

        // 호 형태로 휘두르기
        float elapsed = 0f;
        while (elapsed < swingDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / swingDuration);
            float currentAngle = Mathf.LerpAngle(currentStart, currentEnd, t);
            float rad = currentAngle * Mathf.Deg2Rad;

            sword.position = transform.position + new Vector3(
                Mathf.Cos(rad) * radius,
                Mathf.Sin(rad) * radius,
                0f
            );
            sword.rotation = Quaternion.Euler(0f, 0f, currentAngle);

            if (hitbox != null)
                hitbox.transform.position = sword.position;

            yield return null;
        }

        if (hitbox != null) Destroy(hitbox);

        yield return new WaitForSeconds(_comboCooldown);
    }

    // 피봇 복귀
    if (sword != null && swordIndex >= 0)
    {
        if(orbitSword != null) orbitSword.IsInPattern = false;
        sword.SetParent(_swordPivot);
        sword.localPosition = _swordOriginalLocalPos[swordIndex];
        sword.localRotation = _swordOriginalLocalRot[swordIndex];
    }

    yield return StartCoroutine(MoveToPosition(_originPos, _returnSpeed));
    StartCoroutine(LaunchFreeSwordsSequential());       //후딜 Free 칼 공격
      
    }

    #endregion

    #region 패턴 4 - 위에서 칼 낙하 (2페이즈)

    IEnumerator Pattern4_DropSword()
    {
        if (_player == null) yield break;

        Debug.Log("보스 패턴 4번 실행");

        yield return StartCoroutine(TellRoutine());
        StartCoroutine(LaunchFreeSwordsSequential());       //4번 패턴 시작 시 Free 칼 공격

        int dropCount = Random.Range(_dropCountMin, _dropCountMax + 1);

        if(_isPhase2) dropCount += _dropCountPhase2Bonus;       //2페이즈 +3
        float currentDropSpeed = _isPhase2 ? _dropSpeedPhase2 : _dropSpeed;

        for (int i = 0; i < dropCount; i++)
        {
            float randomX = _player.position.x + Random.Range(-_dropRange, _dropRange);
            Vector3 spawnPos = new Vector3(randomX, _player.position.y + _dropHeight, 0f);

            GameObject dropSword = Instantiate(_dropSwordPrefab, spawnPos, Quaternion.identity);
            StartCoroutine(DropSwordRoutine(dropSword, currentDropSpeed));

            yield return new WaitForSeconds(_dropInterval);
        }
        //마지막 칼이 착지할 시간 대기
        yield return new WaitForSeconds(2f);
    }


    IEnumerator DropSwordRoutine(GameObject sword, float dropSpeed)
    {
        if (sword == null) yield break;
        sword.transform.rotation = Quaternion.Euler(0f, 0f, 180f);

        yield return new WaitForSeconds(_dropWarningDelay);
        //아래로 낙하
        while(true)
        {
            if(sword == null) yield break;

            sword.transform.position += Vector3.down * dropSpeed * Time.deltaTime;

            //바닥 감지
            RaycastHit2D hit = Physics2D.Raycast(sword.transform.position, Vector2.down,
                dropSpeed * Time.deltaTime + 0.3f,
                _dropGroundLayer
            );

            if(hit.collider != null)
            {
                sword.transform.position = new Vector3(
                    sword.transform.position.x, hit.point.y + _dropSwordPivotOffset, 0f
                );

                sword.transform.DOPunchScale(new Vector3(0.2f, -0.3f, 0f), 0.15f, 5, 0.5f);
                break;
            }

            if(sword.transform.position.y < _originPos.y - 10f)
            {
                Destroy(sword);
                yield break;
            }
            yield return null;
        }

        yield return new WaitForSeconds(_dropStickDuration);
        if(sword != null) Destroy(sword);
    }

    #endregion

    #region 패턴 5 - 사방으로 뿌리기 tkqkd 

    IEnumerator Pattern5_BurstSword()
    {
        if (_player == null) yield break;
        int roundCount = _isPhase2 ? 4 : 3;

        Debug.Log("보스 패턴 5번 실행");
        
        yield return StartCoroutine(TellRoutine());
        //StartCoroutine(LaunchFreeSwordsSequential());       //Free 상태 칼 추가공격

        Vector3 burstTarget = _player.position + new Vector3(_burstTargetOffset.x, _burstTargetOffset.y, 0f);
        Vector3 rushTarget = Vector3.MoveTowards(
            transform.position, burstTarget, _burstRushDistance
        );
        yield return StartCoroutine(MoveToPosition(rushTarget, _burstRushSpeed));

        //3번 반복 - 홀수 
        for(int round = 0; round < roundCount; round++)
        {
            float startOffset = (round % 2 == 0) ? 0f : 30f;

            for(int i = 0; i < 6; i++)
            {
                float angle = startOffset + i * 60f;
                float rad = angle * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                if(_burstSwordPrefab != null)
                {
                    GameObject sword = Instantiate(
                        _burstSwordPrefab, transform.position, Quaternion.Euler(0f, 0f, angle -90f)
                    );

                    if(sword.TryGetComponent<Rigidbody2D>(out var rb))
                        rb.linearVelocity = dir * _burstSwordSpeed;
                    else
                        StartCoroutine(MoveSwordRoutine(sword, dir));
                    Destroy(sword, _burstSwordLifetime);
                }
            }   
            yield return new WaitForSeconds(_burstInterval);
        }
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(MoveToPosition(_originPos, _returnSpeed));
        StartCoroutine(LaunchFreeSwordsSequential());       //후딜 Free 칼 공격
    }

    IEnumerator MoveSwordRoutine(GameObject sword, Vector2 dir)
    {
        while(sword != null)
        {
            sword.transform.position += (Vector3)(dir * _burstSwordSpeed * Time.deltaTime);
            yield return null;
        }
    }

    #endregion

    #region 공통 루틴

    IEnumerator TellRoutine()
    {
        SetBodyColor(_colorTell);
        _isImmune = true;
        _currentRotationSpeed *= 0.3f;
        yield return new WaitForSeconds(_tellDuration);
        SetBodyColor(_colorImmune);
        _currentRotationSpeed = _isPhase2 ? _rotationSpeedPhase2 : _rotationSpeedPhase1;
    }

    IEnumerator MoveToPosition(Vector3 target, float speed)
    {
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, target, speed * Time.deltaTime
            );
            yield return null;
        }
        transform.position = target;
    }

    //패턴 실행 시 Free 상태 칼 순차 공격
    IEnumerator LaunchFreeSwordsSequential()
    {
        if(!_isPhase2) yield break;         //? Only Active to sword Attack of Phase2

        foreach(var s in _swords)
        {
            if(s == null) continue;
            var orbitSword = s.GetComponent<Level01_BossOrbitSword>();
            if(orbitSword == null) continue;
            if(orbitSword.State != Level01_BossOrbitSword.SwordState.Free) continue;


            orbitSword.LaunchStraightAttack();
            yield return new WaitForSeconds(_freeSwordLaunchInterval);
        }
    }

    #endregion

    #region 유틸

    Transform GetAvailableSword()
    {
        foreach (var sword in _swords)
        {
            if(sword == null) continue;
            if(sword.parent != _swordPivot) continue;

            var orbitSword = sword.GetComponent<Level01_BossOrbitSword>();
            if(orbitSword != null && orbitSword.State != Level01_BossOrbitSword.SwordState.Orbit)
                continue;
            
            return sword;
        }
            
        return null;
    }

    public int GetSwordIndex(Transform sword)
    {
        for (int i = 0; i < _swords.Length; i++)
            if (_swords[i] == sword) return i;
        return -1;
    }

    void SetBodyColor(Color color)
    {
        if (_bodyRenderer != null)
            _bodyRenderer.color = color;
    }

    #endregion

    
}
