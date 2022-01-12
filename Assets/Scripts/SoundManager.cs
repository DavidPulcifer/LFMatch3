using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    public AudioClip[] musicClips;
    public AudioClip[] winClips;
    public AudioClip[] loseClips;
    public AudioClip[] bonusClips;

    [Range(0, 1)]
    public float musicVolume = 0.5f;

    [Range(0, 1)]
    public float fxVolume = 1.0f;

    [SerializeField] float lowPitch = 0.95f;
    [SerializeField] float highPitch = 1.05f;

    public string musicSourceName = "BackgroundMusic";

    void Start()
    {
        PlayRandomMusic();
    }

    public AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1f, 
                                        bool randomizePitch = true, bool selfDestruct = true)
    {
        if (clip == null) return null;

        GameObject go = new GameObject("SoundFX" + clip.name);
        go.transform.position = position;

        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;

        if (randomizePitch)
        {
            float randomPitch = Random.Range(lowPitch, highPitch);
            source.pitch = randomPitch;
        }       

        source.volume = volume;

        source.Play();
        if (selfDestruct)
        {
            Destroy(go, clip.length);
        }        
        return source;
    }

    public AudioSource PlayRandom(AudioClip[] clips, Vector3 position, float volume = 1f, 
                                    bool randomizePitch = true, bool selfDestruct = true)
    {
        if (clips == null || clips.Length == 0) return null;

        int randomIndex = Random.Range(0, clips.Length);

        if (clips[randomIndex] == null) return null;

        AudioSource source = PlayClipAtPoint(clips[randomIndex], position, volume, randomizePitch, selfDestruct);
        return source;
    }

    public void PlayRandomMusic(bool dontDestroyOnLoad = false)
    {
        GameObject musicObject = GameObject.Find(musicSourceName);

        if (musicObject != null) return;
        
        AudioSource source = PlayRandom(musicClips, Vector3.zero, musicVolume, true, false);
        source.loop = true;
        source.gameObject.name = musicSourceName;

        if(dontDestroyOnLoad && source != null)
        {
            DontDestroyOnLoad(source.gameObject);
        }
    }

    public void PlayWinSound()
    {
        PlayRandom(winClips, Vector3.zero, fxVolume);
    }

    public void PlayLoseSound()
    {
        PlayRandom(loseClips, Vector3.zero, fxVolume);
    }

    public void PlayBonusSound()
    {
        PlayRandom(bonusClips, Vector3.zero, fxVolume);
    }
}
