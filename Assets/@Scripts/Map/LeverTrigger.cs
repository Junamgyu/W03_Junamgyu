using System.Collections;
using UnityEngine;

public class LeverTrigger : MonoBehaviour
{
    private enum LeverType
    {
        moveObj,
        openDoor,
        spawnEnemy,
        changeTile
    }

    [Header("움직이는 발판")]
    [SerializeField] private GameObject _moveObj;
    [SerializeField] private Vector2 _objStartPos;
    [SerializeField] private Vector2 _objEndPos;
    [SerializeField] private float _moveTime;
    private bool isMove;

    [Header("문 여는 레버")]
    [SerializeField] private GameObject _door;
    [SerializeField] private float _closeTime;
    Coroutine runningCo;

    [Header("적 소환 레버")]
    [SerializeField] private GameObject[] _enemySpawners;

    [Header("타일 변환 레버")]
    [SerializeField] private GameObject[] _changeTile_01;
    [SerializeField] private GameObject[] _changeTile_02;
    [SerializeField] private bool _isChangeTile;

    [SerializeField] private LeverType _enemyType;


    private void Start()
    {
        isMove = false;
        _isChangeTile = false;
        LeverFeature();
    }

    private void Update()
    {
        if (_enemyType == LeverType.moveObj)
        {
            if (isMove)
            {
                _moveObj.transform.position = Vector2.MoveTowards(_moveObj.transform.position, _objEndPos, _moveTime);
                if (_moveObj.transform.position.x > _objEndPos.x || _moveObj.transform.position.y > _objEndPos.y)
                {
                    _moveObj.transform.position = _objEndPos;
                }
            }
            else
            {
                _moveObj.transform.position = Vector2.MoveTowards(_moveObj.transform.position, _objStartPos, _moveTime);
                if (_moveObj.transform.position.x < _objStartPos.x || _moveObj.transform.position.y < _objStartPos.y)
                {
                    _moveObj.transform.position = _objStartPos;
                }
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //한 번 맞았을 때 무적 시간을 줄지 말지 아마 줘야 하지 않을까 --> 샷건 때문에
        Bullet bullet = GetComponent<Bullet>();
        if (bullet != null)
        {
            LeverFeature();
        }
    }

    public void LeverFeature()
    {
        switch (_enemyType)
        {
            case LeverType.moveObj:
                if(_moveObj != null) isMove = !isMove;
                break;
            case LeverType.openDoor:
                if (_door != null)
                {
                    _door.SetActive(true);
                    runningCo = StartCoroutine(OpenDoor());
                }
                break;
            case LeverType.spawnEnemy:
                if(_enemySpawners != null)
                {
                    //적 소환 함수 가져오기
                    //_enemySpawners
                }
                break;
            case LeverType.changeTile:
                for (int i = 0; i < _changeTile_01.Length; i++)
                {
                    _changeTile_01[i].SetActive(_isChangeTile);
                }
                _isChangeTile = !_isChangeTile;
                for (int i = 0; i < _changeTile_02.Length; i++)
                {
                    _changeTile_02[i].SetActive(_isChangeTile);
                }
                break;
        }
    }

    IEnumerator OpenDoor()
    {
        yield return new WaitForSeconds(_closeTime);
        _door.SetActive(false);
    }
}