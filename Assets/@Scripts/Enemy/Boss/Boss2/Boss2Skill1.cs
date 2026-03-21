using System.Collections;
using UnityEngine;

public class Boss2Skill1 : MonoBehaviour, ISkill
{
    [Header("레퍼런스")]
    public ChargeBeam chargeBeam;

    [Header("이동 설정")]
    public Transform[] waypoints;
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
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogWarning("Boss2Skill1: waypoint가 2개 이상 필요합니다.");
            yield break;
        }

        _isRunning = true;

        yield return StartCoroutine(MoveToPosition(waypoints[0].position));
        yield return StartCoroutine(chargeBeam.WaitUntilFiring());

        for (int i = 1; i < waypoints.Length; i++)
            yield return StartCoroutine(MoveToPosition(waypoints[i].position));

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