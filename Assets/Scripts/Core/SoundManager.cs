using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip _levelMusic;

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup _musicMixerGroup;
    [SerializeField] private AudioMixerGroup _sfxMixerGroup;
    private void Awake()
    {
        // Singleton pattern (only one SoundManager in the scene)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize audio sources
        if (_musicSource == null)
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.outputAudioMixerGroup = _musicMixerGroup;
            _musicSource.loop = true; // Loop background music
        }

        if (_sfxSource == null)
        {
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.outputAudioMixerGroup = _sfxMixerGroup;
        }
    }
    private void Start()
    {
        PlayLevelMusic();
    }
    public void PlayLevelMusic()
    {
        if (_levelMusic == null)
        {
            Debug.LogWarning("No level music assigned!");
            return;
        }

        _musicSource.clip = _levelMusic;
        _musicSource.Play();
    }
    public void PlaySFX(AudioClip sfxClip)
    {
        if (sfxClip == null)
        {
            Debug.LogWarning("No SFX clip provided!");
            return;
        }

        _sfxSource.PlayOneShot(sfxClip);
    }
    public void SetMusicVolume(float volume)
    {
        _musicSource.volume = Mathf.Clamp01(volume);
    }
    public void SetSFXVolume(float volume)
    {
        _sfxSource.volume = Mathf.Clamp01(volume);
    }
    public void StopMusic()
    {
        _musicSource.Stop();
    }
    public void PauseMusic()
    {
        _musicSource.Pause();
    }
    public void ResumeMusic()
    {
        _musicSource.UnPause();
    }
}