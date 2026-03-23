using System.Collections;
using UnityEngine;

[System.Serializable]
public class WaypointPair
{
    public Transform start;
    public Transform end;
}

public class Boss2Skill1 : MonoBehaviour, ISkill
{
    [Header("레퍼런스")]
    public ChargeBeam chargeBeam;

    [Header("이동 설정")]
    public WaypointPair[] waypointPairs; // 점 쌍 여러 개
    public float moveSpeed = 5f;

    private bool _isRunning = false;
    public bool IsRunning => _isRunning;

    public void StopSkill()
    {
        StopAllCoroutines();
        chargeBeam.StopBeam();
        _isRunning = false;
    }

    public IEnumerator SkillRoutine()
    {
        if (waypointPairs == null || waypointPairs.Length == 0)
        {
            Debug.LogWarning("Boss2Skill1: waypointPair가 1개 이상 필요합니다.");
            yield break;
        }

        _isRunning = true;

        // 랜덤으로 점 쌍 선택
        WaypointPair pair = waypointPairs[Random.Range(0, waypointPairs.Length)];

        if (pair.start == null || pair.end == null)
        {
            Debug.LogWarning("Boss2Skill1: 선택된 pair의 start 또는 end가 null입니다.");
            _isRunning = false;
            yield break;
        }

        // 시작점으로 이동
        yield return StartCoroutine(MoveToPosition(pair.start.position));

        // 레이저 발사까지 대기
        yield return StartCoroutine(chargeBeam.WaitUntilFiring());

        // 끝점으로 이동
        yield return StartCoroutine(MoveToPosition(pair.end.position));

        // 레이저 끄기
        chargeBeam.StopBeam();
        yield return new WaitForSeconds(chargeBeam.shrinkTime);

        _isRunning = false;
    }

    IEnumerator MoveToPosition(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
    }
}