using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Mixer (Optional)")]
    [Tooltip("Route music to this mixer group (optional).")]
    public AudioMixerGroup musicMixer;
    [Tooltip("Route SFX to this mixer group (optional).")]
    public AudioMixerGroup sfxMixer;

    [Header("Music")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    public AudioSource musicA;
    public AudioSource musicB;
    private AudioSource _activeMusic;   // currently playing
    private AudioSource _idleMusic;     // for crossfades
    private Coroutine _musicFadeCo;

    [Header("SFX Pool")]
    [Range(1, 32)] public int sfxPoolSize = 10;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    private readonly List<AudioSource> _sfxPool = new();
    private int _sfxIndex;

    [Header("SFX Library (Inspector-Defined)")]
    [Tooltip("Define simple keys for quick SFX lookup, e.g., 'click', 'hit', 'heal'.")]
    public List<SfxEntry> sfxList = new();   // inspector-visible list
    private readonly Dictionary<string, AudioClip> _sfxMap = new();

    [System.Serializable]
    public struct SfxEntry
    {
        public string key;
        public AudioClip clip;
    }

    private void Awake()
    {
        // Singleton
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject);

        // Ensure music sources exist (or create them)
        if (musicA == null) musicA = CreateChildSource("Music A", isMusic: true);
        if (musicB == null) musicB = CreateChildSource("Music B", isMusic: true);
        _activeMusic = musicA;
        _idleMusic = musicB;

        // Build SFX pool
        EnsureSfxPool();

        // Build key->clip map
        _sfxMap.Clear();
        foreach (var e in sfxList)
        {
            if (!string.IsNullOrWhiteSpace(e.key) && e.clip != null)
                _sfxMap[e.key] = e.clip;
        }
    }

    private AudioSource CreateChildSource(string name, bool isMusic)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = isMusic;
        src.spatialBlend = isMusic ? 0f : 0f; // music always 2D, SFX default 2D here
        src.outputAudioMixerGroup = isMusic ? musicMixer : sfxMixer;
        return src;
    }

    private void EnsureSfxPool()
    {
        // Clear existing children designated as SFX if any
        // (We won’t destroy; we can reuse if count matches)
        while (_sfxPool.Count < sfxPoolSize)
        {
            var src = CreateChildSource($"SFX {_sfxPool.Count}", isMusic: false);
            _sfxPool.Add(src);
        }
    }

    // -------- MUSIC --------

    /// <summary>Plays music immediately with no fade.</summary>
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        if (_musicFadeCo != null) StopCoroutine(_musicFadeCo);

        _activeMusic.clip = clip;
        _activeMusic.loop = loop;
        _activeMusic.volume = musicVolume;
        _activeMusic.Play();

        // Stop the idle track if it was doing something
        _idleMusic.Stop();
        _idleMusic.clip = null;
    }

    /// <summary>Crossfades to new music over fadeDuration seconds.</summary>
    public void CrossfadeMusic(AudioClip newClip, float fadeDuration = 1f, bool loop = true)
    {
        if (newClip == null) return;

        if (_musicFadeCo != null) StopCoroutine(_musicFadeCo);
        _musicFadeCo = StartCoroutine(Co_CrossfadeMusic(newClip, fadeDuration, loop));
    }

    private IEnumerator Co_CrossfadeMusic(AudioClip newClip, float fadeDuration, bool loop)
    {
        // Prepare idle source with the next track
        _idleMusic.clip = newClip;
        _idleMusic.loop = loop;
        _idleMusic.volume = 0f;
        _idleMusic.Play();

        float t = 0f;
        float startVol = _activeMusic.volume;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = fadeDuration <= 0f ? 1f : Mathf.Clamp01(t / fadeDuration);
            _activeMusic.volume = Mathf.Lerp(startVol, 0f, k);
            _idleMusic.volume = Mathf.Lerp(0f, musicVolume, k);
            yield return null;
        }

        // Swap roles
        _activeMusic.Stop();
        var tmp = _activeMusic;
        _activeMusic = _idleMusic;
        _idleMusic = tmp;

        _activeMusic.volume = musicVolume;
        _idleMusic.clip = null;
        _musicFadeCo = null;
    }

    /// <summary>Stops music with optional fade-out.</summary>
    public void StopMusic(float fadeOut = 0.2f)
    {
        if (_musicFadeCo != null) StopCoroutine(_musicFadeCo);
        StartCoroutine(Co_StopMusic(fadeOut));
    }

    private IEnumerator Co_StopMusic(float fadeOut)
    {
        float t = 0f;
        float startVol = _activeMusic.volume;

        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            float k = fadeOut <= 0f ? 1f : Mathf.Clamp01(t / fadeOut);
            _activeMusic.volume = Mathf.Lerp(startVol, 0f, k);
            yield return null;
        }

        _activeMusic.Stop();
        _activeMusic.volume = musicVolume;
    }

    public bool IsMusicPlaying => _activeMusic != null && _activeMusic.isPlaying;

    // -------- SFX --------

    /// <summary>Play a one-shot SFX by key from the library.</summary>
    public void PlaySFX(string key, float volume = 1f, float pitch = 1f)
    {
        if (!_sfxMap.TryGetValue(key, out var clip) || clip == null) return;
        PlaySFX(clip, volume, pitch);
    }

    /// <summary>Play a one-shot SFX by AudioClip.</summary>
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        var src = NextSfxSource();
        src.transform.position = Vector3.zero;
        src.spatialBlend = 0f; // 2D
        PlayOn(src, clip, volume * sfxVolume, pitch);
    }

    /// <summary>Play a 3D SFX at world position by key.</summary>
    public void PlaySFXAt(string key, Vector3 position, float volume = 1f, float pitch = 1f, float spatialBlend = 1f)
    {
        if (!_sfxMap.TryGetValue(key, out var clip) || clip == null) return;
        PlaySFXAt(clip, position, volume, pitch, spatialBlend);
    }

    /// <summary>Play a 3D SFX at world position by clip.</summary>
    public void PlaySFXAt(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, float spatialBlend = 1f)
    {
        if (clip == null) return;
        var src = NextSfxSource();
        src.transform.position = position;
        src.spatialBlend = Mathf.Clamp01(spatialBlend); // 1 = fully 3D
        PlayOn(src, clip, volume * sfxVolume, pitch);
    }

    private AudioSource NextSfxSource()
    {
        if (_sfxPool.Count == 0) EnsureSfxPool();
        _sfxIndex = (_sfxIndex + 1) % _sfxPool.Count;
        var src = _sfxPool[_sfxIndex];
        if (src == null)
        {
            // Safety: recreate if something got destroyed
            src = CreateChildSource($"SFX {_sfxIndex}", isMusic: false);
            _sfxPool[_sfxIndex] = src;
        }
        return src;
    }

    private static void PlayOn(AudioSource src, AudioClip clip, float volume, float pitch)
    {
        src.pitch = Mathf.Clamp(pitch, -3f, 3f);
        src.volume = Mathf.Clamp01(volume);
        src.loop = false;
        src.clip = clip;
        src.Stop();
        src.Play();
    }

    // -------- Convenience --------

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        if (_activeMusic != null) _activeMusic.volume = musicVolume;
    }

    public void SetSfxVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        // Pool volumes are applied at play time; nothing to do proactively
    }
}
