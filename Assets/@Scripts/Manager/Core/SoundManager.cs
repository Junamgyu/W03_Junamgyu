//using Unity.VectorGraphics.Editor;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }

    [SerializeField] private BGMPlayer _bgmPlayer;
    [SerializeField] private SFXPlayer _sfxPlayer;
    [Tooltip("SO관리 만들기 귀찮기에 배열 관리로 함\n원한다면 DataBase운용으로 바꿔도 상관X\nSO가 만들어 져있기는 한데 clip만 넣고 따로 key안 만들어둠")]
    [SerializeField] private BGM[] _bgmEntry;
    [SerializeField] private SFX[] _sfxEntry;

    [Tooltip("볼륨 조절 따로 안 만드니깐 적당한 소리로 조절 필요")]
    [SerializeField] private float _bgmVolume = 1f;
    [SerializeField] private float _sfxVolume = 1f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void PlayBGM(int number, bool fade = true)
    {
        AudioClip clip = _bgmEntry[number]._clip;
        _bgmPlayer.Play(clip, fade, _bgmVolume);
    }

    private void PlaySFX(int number)
    {
        AudioClip clip = _sfxEntry[number]._clip;
        _sfxPlayer.Play(clip, _sfxVolume);
    }

    //스테이지 시작시 이벤트 걸어주세요
    public void HandleMainStart()
    {
        PlayBGM(0);
    }
    //보스 입장시 이벤트 걸어주세요
    public void HandleBossStart()
    {
        PlayBGM(1);
    }

    private void HandleAttack()
    {

    }
}
