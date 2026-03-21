using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss2Controller : MonoBehaviour
{
    [Header("스킬")]
    public MonoBehaviour[] skills; // ISkill 구현체들 Inspector에서 연결

    [Header("패턴 설정")]
    public float idleDuration = 2f;
    public float phaseStartDelay = 1.5f;

    private bool _isActive = false;
    private List<ISkill> _skills = new List<ISkill>();

    void Awake()
    {
        // MonoBehaviour 배열에서 ISkill만 추출
        foreach (var skill in skills)
        {
            if (skill is ISkill s)
                _skills.Add(s);
            else
                Debug.LogWarning($"{skill.name}은 ISkill을 구현하지 않았습니다.");
        }
    }

    void Start()
    {
        StartBoss2();
    }

    public void StartBoss2()
    {
        _isActive = true;
        StartCoroutine(DelayedStart());
    }

    public void StopBoss2()
    {
        _isActive = false;
        StopAllCoroutines();
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(phaseStartDelay);
        StartCoroutine(PatternCycleRoutine());
    }

    IEnumerator PatternCycleRoutine()
    {
        while (_isActive)
        {
            ISkill skill = PickRandomSkill();
            if (skill != null)
                yield return StartCoroutine(skill.SkillRoutine());

            yield return new WaitForSeconds(idleDuration);
        }
    }

    ISkill PickRandomSkill()
    {
        if (_skills.Count == 0) return null;
        return _skills[Random.Range(0, _skills.Count)];
    }
}