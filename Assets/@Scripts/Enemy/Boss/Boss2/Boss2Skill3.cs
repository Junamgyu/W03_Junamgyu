using System.Collections;
using UnityEngine;

public class Boss2Skill3 : MonoBehaviour, ISkill
{
    [Header("이동 설정")]
    public Transform pointA;
    public Transform pointB;
    public float dashSpeed = 20f;  // 빠르게 지나가는 속도
    public int repeatCount = 2;     // 왕복 횟수

    [Header("대미지 설정")]
    public int contactDamage = 20;    // 플레이어 충돌 시 대미지

    private bool _isDashing = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!_isDashing) return;
        if (col.CompareTag("Player"))
            col.GetComponent<IDamageable>()?.TakeDamage(contactDamage);
    }

    public IEnumerator SkillRoutine()
    {
        _isDashing = true;

        // A → B → A → B ... repeatCount만큼 왕복
        Transform current = pointA;
        int totalMoves = repeatCount * 2; // 왕복 1회 = 2번 이동

        for (int i = 0; i < totalMoves; i++)
        {
            yield return StartCoroutine(DashToPosition(current.position));
            current = current == pointA ? pointB : pointA;
        }

        _isDashing = false;
    }

    IEnumerator DashToPosition(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, dashSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
    }
}