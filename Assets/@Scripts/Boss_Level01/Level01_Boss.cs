using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Level01_Boss : EnemyBase
{
    #region 인스펙터 변수

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

    [Header("패턴 1 - 칼 던지기")]
    [SerializeField] private float _throwSpeed = 12f;
    [SerializeField] private float _throwReturnDelay = 0.8f;
    [SerializeField] private float _throwReturnSpeed = 16f;

    [Header("패턴 2 - 내려찍기")]
    [SerializeField] private float _slamRiseSpeed = 8f;
    [SerializeField] private float _slamSpeed = 18f;
    [SerializeField] private float _slamHeight = 4f;
    [SerializeField] private GameObject _shockwavePrefab;

    [Header("패턴 3 - 칼 콤보")]
    [SerializeField] private float _comboMoveSpeed = 12f;
    [SerializeField] private float _comboCooldown = 0.25f;
    [SerializeField] private GameObject _slashHitboxPrefab;

    [Header("패턴 4 - 칼 낙하")]
    [SerializeField] private float _dropSpeed = 20f;
    [SerializeField] private int _dropCount = 3;
    [SerializeField] private float _dropRange = 4f;
    [SerializeField] private float _dropInterval = 0.3f;
    [SerializeField] private float _dropHeight = 7f;

    [Header("패턴 5 - 바닥 칼")]
    [SerializeField] private GameObject _groundSpikePrefab;
    [SerializeField] private int _spikeCount = 4;
    [SerializeField] private float _spikeRange = 5f;
    [SerializeField] private float _spikeInterval = 0.4f;
    [SerializeField] private float _spikeRiseSpeed = 8f;

    [Header("피격 연출")]
    [SerializeField] private int _hitFlashCount = 3;
    [SerializeField] private float _hitFlashInterval = 0.08f;

    #endregion

    #region 색깔 상태
    private readonly Color _colorIdle = Color.white;
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

    // 칼 원래 로컬 위치 저장
    private Vector3[] _swordOriginalLocalPos;

    #endregion

    #region 생명주기

    protected override void Start()
    {
        base.Start();
        _originPos = transform.position;
        _currentRotationSpeed = _rotationSpeedPhase1;

        //칼 원래 위치 저장
        _swordOriginalLocalPos = new Vector3[_swords.Length];
        for(int i = 0; i < _swords.Length; i++)
        {
            if(_swords[i] != null)
            {
                _swordOriginalLocalPos[i] = _swords[i].localPosition;
            }
        }
        GameObject playerObj = GameObject.FindWithTag("Player");
        if(playerObj != null) _player = playerObj.transform;

        SetBodyColor(_colorIdle);
        _patternCoroutine = StartCoroutine(PatternCycleRoutine());
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Update is called once per frame
    void Update()
    {
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
        StartCoroutine(RunPattern(Pattern5_GroundSword()));
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

    #endregion

    #region 피격 / 사망
    public override void TakeDamage(int damage, bool isAddGauge = false)
    {
        if(_isImmune) return;

        if(_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(HitFlashRoutine());

        base.TakeDamage(damage, isAddGauge);

        if(!_isPhase2 && _currentHp <= _maxHp * 0.05f)
            StartCoroutine(EnterPhase2Routine());
    }

    public override void Die()
    {
        StopAllCoroutines();
        StartCoroutine(BossDieRoutine());
    }

    //죽었을 경우
    IEnumerator BossDieRoutine()
    {
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
    }

    IEnumerator HitFlashRoutine()
    {
        Color original = _bodyRenderer.color;
        for(int i = 0; i < _hitFlashCount; i++)
        {
            _bodyRenderer.color = Color.white;
            yield return new WaitForSeconds(_hitFlashInterval);
            _bodyRenderer.color = original;
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

        Transform sword = GetAvailableSword();
        if(sword == null) yield break;

        //칼을 피봇에서 분리
        int sowrdIndex = GetSwordIndex(sword);
        sword.SetParent(null);

        Vector2 dir = (_player.position - sword.position).normalized;
        float elapsed = 0f;

        //날아가기
        while(elapsed < _throwReturnDelay)
        {
            sword.position += (Vector3)(dir * _throwSpeed * Time.deltaTime);
            sword.Rotate(0f, 0f, -720f * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        //돌아오기
        float returnElapsed = 0f;
        while(returnElapsed < 3f)
        {
            if(sword == null) break;

            Vector2 toOwner = (transform.position - sword.position).normalized;
            sword.position += (Vector3)(toOwner * _throwReturnSpeed * Time.deltaTime);
            sword.Rotate(0f, 0f, -720f * Time.deltaTime);
            returnElapsed += Time.deltaTime;

            if(Vector3.Distance(sword.position, transform.position) < 0.5f)
                break;
            
            yield return null;
        }

        //피봇 복귀
        if(sword != null && sowrdIndex >= 0)
        {
            sword.SetParent(_swordPivot);
            sword.localPosition = _swordOriginalLocalPos[sowrdIndex];
            sword.localRotation = Quaternion.identity;
        }
    }
    #endregion

    #region 패턴 2 - 내려 찍기 (가드 불가 / 2페이즈 충격파 가드 가능)

    IEnumerator Pattern2_Slam()
    {
        if (_player == null) yield break;

        yield return StartCoroutine(TellRoutine());

        // 플레이어 위로 이동
        Vector3 riseTarget = new Vector3(
            _player.position.x,
            _player.position.y + _slamHeight,
            0f
        );
        yield return StartCoroutine(MoveToPosition(riseTarget, _slamRiseSpeed));

        // 예고 멈춤
        SetBodyColor(Color.red);
        yield return new WaitForSeconds(0.4f);

        // 내려찍기
        Vector3 slamTarget = new Vector3(_player.position.x, _player.position.y, 0f);
        yield return StartCoroutine(MoveToPosition(slamTarget, _slamSpeed));

        // 충격 연출
        transform.DOPunchScale(new Vector3(0.4f, -0.4f, 0f), 0.2f, 5, 0.5f);

        // 2페이즈 충격파
        if (_isPhase2 && _shockwavePrefab != null)
            Instantiate(_shockwavePrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(MoveToPosition(_originPos, _returnSpeed));
    }

    #endregion

    #region 패턴 3 - 칼 3회 콤보 (가드 가능)

    IEnumerator Pattern3_SwordCombo()
    {
        if (_player == null) yield break;

        yield return StartCoroutine(TellRoutine());

        for (int i = 0; i < 3; i++)
        {
            Vector3 attackPos = Vector3.MoveTowards(
                transform.position, _player.position, 2f
            );
            yield return StartCoroutine(MoveToPosition(attackPos, _comboMoveSpeed));

            // 히트박스 생성
            if (_slashHitboxPrefab != null)
            {
                GameObject hitbox = Instantiate(
                    _slashHitboxPrefab,
                    transform.position,
                    Quaternion.identity
                );
                Destroy(hitbox, 0.15f);
            }

            transform.DOPunchPosition(
                (_player.position - transform.position).normalized * 0.4f,
                0.15f, 5, 0.5f
            );

            yield return new WaitForSeconds(_comboCooldown);
        }

        yield return StartCoroutine(MoveToPosition(_originPos, _returnSpeed));
    }

    #endregion

    #region 패턴 4 - 위에서 칼 낙하 (2페이즈)

    IEnumerator Pattern4_DropSword()
    {
        if (_player == null) yield break;

        yield return StartCoroutine(TellRoutine());

        for (int i = 0; i < _dropCount; i++)
        {
            float randomX = _player.position.x + Random.Range(-_dropRange, _dropRange);
            Vector3 spawnPos = new Vector3(randomX, _player.position.y + _dropHeight, 0f);

            Transform sword = GetAvailableSword();
            if (sword != null)
            {
                int idx = GetSwordIndex(sword);
                sword.SetParent(null);
                sword.position = spawnPos;
                StartCoroutine(DropSwordRoutine(sword, idx));
            }

            yield return new WaitForSeconds(_dropInterval);
        }

        yield return new WaitForSeconds(1.5f);
    }

    IEnumerator DropSwordRoutine(Transform sword, int originalIndex)
    {
        float elapsed = 0f;
        while (elapsed < 3f)
        {
            if (sword == null) yield break;

            sword.position += Vector3.down * _dropSpeed * Time.deltaTime;
            sword.Rotate(0f, 0f, -360f * Time.deltaTime);
            elapsed += Time.deltaTime;

            // 바닥 도달 시 멈춤
            if (sword.position.y <= _originPos.y)
                break;

            yield return null;
        }

        yield return new WaitForSeconds(1f);

        // 피봇 복귀
        if (sword != null && originalIndex >= 0)
        {
            sword.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
            {
                sword.SetParent(_swordPivot);
                sword.localScale = Vector3.one;
                sword.localPosition = _swordOriginalLocalPos[originalIndex];
                sword.localRotation = Quaternion.identity;
            });
        }
    }

    #endregion

    #region 패턴 5 - 바닥에서 칼 솟아오르기 (2페이즈)

    IEnumerator Pattern5_GroundSword()
    {
        if (_player == null) yield break;

        yield return StartCoroutine(TellRoutine());

        for (int i = 0; i < _spikeCount; i++)
        {
            float offsetX = (i - _spikeCount / 2f) * (_spikeRange / _spikeCount);
            Vector3 spikeBase = new Vector3(
                _player.position.x + offsetX,
                _originPos.y - 3f,
                0f
            );

            if (_groundSpikePrefab != null)
            {
                GameObject spike = Instantiate(_groundSpikePrefab, spikeBase, Quaternion.identity);
                StartCoroutine(SpikeRiseRoutine(spike));
            }

            yield return new WaitForSeconds(_spikeInterval);
        }

        yield return new WaitForSeconds(1.5f);
    }

    IEnumerator SpikeRiseRoutine(GameObject spike)
    {
        Vector3 riseTarget = spike.transform.position + Vector3.up * 5f;

        while (Vector3.Distance(spike.transform.position, riseTarget) > 0.05f)
        {
            spike.transform.position = Vector3.MoveTowards(
                spike.transform.position, riseTarget, _spikeRiseSpeed * Time.deltaTime
            );
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        Destroy(spike);
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

    #endregion

    #region 유틸

    Transform GetAvailableSword()
    {
        foreach (var sword in _swords)
            if (sword != null && sword.parent == _swordPivot)
                return sword;
        return null;
    }

    int GetSwordIndex(Transform sword)
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
