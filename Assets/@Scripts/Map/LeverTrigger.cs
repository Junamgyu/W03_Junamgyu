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

    [Header("�����̴� ����")]
    [SerializeField] private GameObject _moveObj;
    [SerializeField] private Vector2 _objStartPos;
    [SerializeField] private Vector2 _objEndPos;
    [SerializeField] private float _moveTime;
    private bool isMove;

    [Header("�� ���� ����")]
    [SerializeField] private GameObject[] _door;
    [SerializeField] private float _closeTime;
    Coroutine runningCo;

    [Header("�� ��ȯ ����")]
    [SerializeField] private GameObject[] _enemySpawners;

    [Header("Ÿ�� ��ȯ ����")]
    [SerializeField] private GameObject[] _changeTile_01;
    [SerializeField] private GameObject[] _changeTile_02;
    [SerializeField] private bool _isChangeTile;

    [SerializeField] private LeverType _enemyType;


    private void Start()
    {
        isMove = false;
        _isChangeTile = false;
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
        //�� �� �¾��� �� ���� �ð��� ���� ���� �Ƹ� ��� ���� ������ --> ���� ������
        Bullet bullet = collision.GetComponent<Bullet>();
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
                    for(int i = 0; i < _door.Length; i++)
                    {
                        _door[i].SetActive(true);
                    }
                    runningCo = StartCoroutine(OpenDoor());
                }
                break;
            case LeverType.spawnEnemy:
                if(_enemySpawners != null)
                {
                    //�� ��ȯ �Լ� ��������
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
        for(int i = 0; i < _door.Length; i++)
        {
            _door[i].SetActive(false);    
        }
        
    }
}