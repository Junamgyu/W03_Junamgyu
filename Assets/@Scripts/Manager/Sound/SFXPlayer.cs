using System.Collections.Generic;
using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    [SerializeField] private int _poolSize = 10;
    [SerializeField] private AudioSource sourcePrefab;

    private List<AudioSource> _soundPool = new List<AudioSource>();

    private void Awake()
    {
        for (int i = 0; i < _poolSize; i++)
        {
            AudioSource source = Instantiate(sourcePrefab, transform);
            _soundPool.Add(source);
        }
    }

    public void Play(AudioClip clip, float volume)
    {
        AudioSource source = GetAvailableSource();
        source.volume = volume;
        source.pitch = 1f;
        source.PlayOneShot(clip);
    }

    private AudioSource GetAvailableSource()
    {
        foreach (var source in _soundPool)
        {
            if (!source.isPlaying)
                return source;
        }

        return _soundPool[0];
    }
}
