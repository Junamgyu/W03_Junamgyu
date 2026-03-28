using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Level01_BossHealth : MonoBehaviour
{
    [Header("칼 연결")]
    [SerializeField] private Transform[] _swords;

    [Header("피격 연출")]
    [SerializeField] private SpriteRenderer _bodyRenderer;
    [SerializeField] private int _hitFlashCount = 3;
    [SerializeField] private float _hitFlashInterval = 0.08f;

    private Level01_Boss _boss;
    private Coroutine _flashCoroutine;

    private void Awake()
    {
        _boss = GetComponent<Level01_Boss>();
    }

    public void OnHit()
    {
        if(_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(HitFlashRoutine());
        UpdateOrbitSwords();
    }

    void UpdateOrbitSwords()
    {
        float hpRatio = (float)_boss.CurrentHp / _boss.MaxHp;
        int activeSwordCount = Mathf.CeilToInt(hpRatio * _swords.Length);

        for(int i = 0; i < _swords.Length; i++)
        {
            if(_swords[i] == null) continue;

            if(i >= activeSwordCount && _swords[i].gameObject.activeSelf)
                StartCoroutine(RemoveSwordRoutine(_swords[i]));
        }
    }

    IEnumerator RemoveSwordRoutine(Transform sword)
    {
        if(sword == null) yield break;

        sword.SetParent(null);

        Vector2 dir = (sword.position - transform.position).normalized;
        sword.DOMove(sword.position + (Vector3)(dir * 4f), 0.4f).SetEase(Ease.OutQuad);
        sword.DORotate(new Vector3(0f, 0f, Random.Range(180f, 360f)), 0.4f);

        yield return new WaitForSeconds(0.3f);

        SpriteRenderer sr = sword.GetComponent<SpriteRenderer>();
        if(sr != null)
            sr.DOFade(0f, 0.2f).OnComplete(() => sword.gameObject.SetActive(false));
        else
            sword.gameObject.SetActive(false);
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

}
