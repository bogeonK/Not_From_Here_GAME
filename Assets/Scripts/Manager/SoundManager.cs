using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BgmId
{
    None,
    Lobby,
    Town,
    Dungeon,
    Battle,
    EndingGood,
    EndingBad
}

public enum SfxId
{
    None,
    UiClick,
    UiHover,
    Confirm,
    Cancel,
    Portal,
    BattleStart,
    Hit,
    Heal,
    BadEndingSting
}

public class SoundManager : baseManager
{
    private readonly SoundManagerConfigSO _config;

    private GameObject _host;
    private AudioSource _bgmA;
    private AudioSource _bgmB;
    private AudioSource _sfx;

    private AudioSource _currentBgm;
    private AudioSource _nextBgm;

    private Coroutine _fadeRoutine;
    private BgmId _currentBgmId = BgmId.None;

    private readonly Stack<BgmId> _bgmStack = new();

    private readonly Dictionary<string, BgmId> _sceneToBgm = new();


    public SoundManager(SoundManagerConfigSO config)
    {
        _config = config;
    }

    public override void Init()
    {
        if (_config == null)
        {
            return;
        }

        EnsureHost();
        BuildSceneMap();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        var active = SceneManager.GetActiveScene().name;
        if (_sceneToBgm.TryGetValue(active, out var bgm))
            PlayBgm(bgm, 0f, true);
        else
            PlayBgm(_config.defaultBgm, 0f, true);
    }

    public override void ActiveOff()
    {

    }

    public override void Update()
    {

    }

    public override void Destory()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (_host != null)
            Object.Destroy(_host);
    }

    private void EnsureHost()
    {
        if (_host != null) return;

        _host = new GameObject("[SoundHost]");
        Object.DontDestroyOnLoad(_host);

        _bgmA = _host.AddComponent<AudioSource>();
        _bgmB = _host.AddComponent<AudioSource>();
        _sfx = _host.AddComponent<AudioSource>();

        _bgmA.playOnAwake = false;
        _bgmB.playOnAwake = false;
        _sfx.playOnAwake = false;

        _bgmA.loop = true;
        _bgmB.loop = true;

        _bgmA.volume = 0f;
        _bgmB.volume = 0f;

        _currentBgm = _bgmA;
        _nextBgm = _bgmB;
    }

    private void BuildSceneMap()
    {
        _sceneToBgm.Clear();
        foreach (var e in _config.sceneBgms)
        {
            if (e == null) continue;
            if (string.IsNullOrEmpty(e.sceneName)) continue;
            if (e.bgm == BgmId.None) continue;
            _sceneToBgm[e.sceneName] = e.bgm;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _bgmStack.Clear();

        if (_sceneToBgm.TryGetValue(scene.name, out var bgm))
            PlayBgm(bgm);
        else
            PlayBgm(_config.defaultBgm);
    }

    public void PlaySfx(SfxId id)
    {
        if (id == SfxId.None) return;
        if (!TryGetSfx(id, out var clip, out var vol)) return;

        _sfx.PlayOneShot(clip, vol * _config.sfxVolume * _config.masterVolume);
    }

    public void PlayBgm(BgmId id, float fadeSeconds = -1f, bool forceRestart = false)
    {
        if (id == BgmId.None)
        {
            StopBgm(fadeSeconds);
            return;
        }

        if (!forceRestart && _currentBgmId == id) return;

        if (fadeSeconds < 0f) fadeSeconds = _config.defaultFadeSeconds;

        if (!TryGetBgm(id, out var clip, out var entryVol, out var loop))
        {
            Debug.LogWarning($"[SoundManager] Missing BGM clip for {id}");
            return;
        }

        _currentBgmId = id;

        float targetTo = entryVol * _config.bgmVolume * _config.masterVolume;

        if (fadeSeconds <= 0f)
        {
            if (_fadeRoutine != null) GameController.instance.StopCoroutine(_fadeRoutine);
            _fadeRoutine = null;

            // 기존 BGM 정리
            if (_currentBgm != null)
            {
                _currentBgm.Stop();
                _currentBgm.clip = null;
                _currentBgm.volume = 0f;
            }

            _nextBgm.clip = clip;
            _nextBgm.loop = loop;
            _nextBgm.volume = targetTo;
            _nextBgm.Play();

            var tmp = _currentBgm;
            _currentBgm = _nextBgm;
            _nextBgm = tmp;

            return;
        }

        _nextBgm.clip = clip;
        _nextBgm.loop = loop;
        _nextBgm.volume = 0f;
        _nextBgm.Play();

        if (_fadeRoutine != null) GameController.instance.StopCoroutine(_fadeRoutine);
        _fadeRoutine = GameController.instance.StartCoroutine(CrossFadeRoutine(_currentBgm, _nextBgm, targetTo, fadeSeconds));
    }

    public void StopBgm(float fadeSeconds = -1f)
    {
        if (fadeSeconds < 0f) fadeSeconds = _config.defaultFadeSeconds;

        if (_fadeRoutine != null) GameController.instance.StopCoroutine(_fadeRoutine);
        _fadeRoutine = GameController.instance.StartCoroutine(StopBgmRoutine(fadeSeconds));

        _currentBgmId = BgmId.None;
        _bgmStack.Clear();
    }


    public void PushBgm(BgmId id, float fadeSeconds = -1f)
    {
        if (_currentBgmId != BgmId.None)
            _bgmStack.Push(_currentBgmId);

        PlayBgm(id, fadeSeconds);
    }

    public void PopBgm(float fadeSeconds = -1f)
    {
        if (_bgmStack.Count <= 0) return;
        var prev = _bgmStack.Pop();
        PlayBgm(prev, fadeSeconds);
    }

    public void SetAreaBgm(BgmId id, float fadeSeconds = -1f)
    {
        PlayBgm(id, fadeSeconds);
    }
    private bool TryGetBgm(BgmId id, out AudioClip clip, out float volume, out bool loop)
    {
        foreach (var e in _config.bgms)
        {
            if (e == null) continue;
            if (e.id != id) continue;
            if (e.clip == null) continue;
            clip = e.clip;
            volume = e.volume;
            loop = e.loop;
            return true;
        }
        clip = null;
        volume = 1f;
        loop = true;
        return false;
    }

    private bool TryGetSfx(SfxId id, out AudioClip clip, out float volume)
    {
        foreach (var e in _config.sfxs)
        {
            if (e == null) continue;
            if (e.id != id) continue;
            if (e.clip == null) continue;
            clip = e.clip;
            volume = e.volume;
            return true;
        }
        clip = null;
        volume = 1f;
        return false;
    }

    private IEnumerator CrossFadeRoutine(AudioSource from, AudioSource to, float targetTo, float duration)
    {
        float startFrom = from != null && from.isPlaying ? from.volume : 0f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);

            if (from != null && from.isPlaying)
                from.volume = Mathf.Lerp(startFrom, 0f, a);

            if (to != null)
                to.volume = Mathf.Lerp(0f, targetTo, a);

            yield return null;
        }

        if (from != null && from.isPlaying)
        {
            from.Stop();
            from.clip = null;
            from.volume = 0f;
        }

        var tmp = _currentBgm;
        _currentBgm = to;
        _nextBgm = tmp;

        _fadeRoutine = null;
    }

    private IEnumerator StopBgmRoutine(float duration)
    {
        float start = _currentBgm != null ? _currentBgm.volume : 0f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            if (_currentBgm != null) _currentBgm.volume = Mathf.Lerp(start, 0f, a);
            yield return null;
        }

        if (_currentBgm != null) { _currentBgm.Stop(); _currentBgm.clip = null; _currentBgm.volume = 0f; }
        if (_nextBgm != null) { _nextBgm.Stop(); _nextBgm.clip = null; _nextBgm.volume = 0f; }

        _fadeRoutine = null;
    }
}