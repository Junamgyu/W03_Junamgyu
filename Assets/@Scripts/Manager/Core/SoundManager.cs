//using Unity.VectorGraphics.Editor;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }


    [SerializeField] private BGMPlayer _bgmPlayer;
    [SerializeField] private SFXPlayer _sfxPlayer;
    [Tooltip("SO���� ����� �����⿡ �迭 ������ ��\n���Ѵٸ� DataBase������� �ٲ㵵 ���X\nSO�� ����� ���ֱ�� �ѵ� clip�� �ְ� ���� key�� ������")]
    [SerializeField] private BGM[] _bgmEntry;
    [SerializeField] private SFX[] _sfxEntry;

    [Tooltip("���� ���� ���� �� ����ϱ� ������ �Ҹ��� ���� �ʿ�")]
    [SerializeField] private float _bgmVolume = 1f;
    [SerializeField] private float _sfxVolume = 1f;

    void OnEnable()
    {
        CameraManager.OnBossOutro += HandleBossStart;

    }

    void OnDisable()
    {
        CameraManager.OnBossOutro -= HandleBossStart;


    }

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

    private void Start()
    {
        PlayBGM(0);
    }
    private void PlayBGM(int number, bool fade = true)
    {
        AudioClip clip = _bgmEntry[number]._clip;
        _bgmPlayer.Play(clip, fade, _bgmVolume);
    }
    public void StopBGM()
    {
        _bgmPlayer.Stop();
        Debug.Log("사운드");
    }
    private void PlaySFX(int number)
    {
        AudioClip clip = _sfxEntry[number]._clip;
        _sfxPlayer.Play(clip, _sfxVolume);
    }

    //�������� ���۽� �̺�Ʈ �ɾ��ּ���
    public void HandleMainStart()
    {
        PlayBGM(0);
    }
    //���� ����� �̺�Ʈ �ɾ��ּ���
    public void HandleBossStart()
    {
        PlayBGM(1);
    }

    private void HandlePlayerHitSFX()
    {
        PlaySFX(0);
    }
    private void HandleEnemyHitSFX()
    {
        PlaySFX(1);
    }
    private void HandleDeadEyeSFX()
    {
        PlaySFX(2);
    }
    private void HandlePistolSFX()
    {
        PlaySFX(3);
    }
    private void HandleShotGunSFX()
    {
        PlaySFX(4);
    }
    private void HandleTNTSFX()
    {
        PlaySFX(5);
    }
}
