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

    // 호스트 오브젝트
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
            Debug.LogError("[SoundManager] Config is null!");
            return;
        }

        EnsureHost();
        BuildSceneMap();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 시작 BGM: 현재 씬 기준으로 자동 매칭, 없으면 default
        var active = SceneManager.GetActiveScene().name;
        if (_sceneToBgm.TryGetValue(active, out var bgm))
            PlayBgm(bgm, 0f, true);
        else
            PlayBgm(_config.defaultBgm, 0f, true);
    }

    public override void ActiveOff()
    {
        // 게임 시작 시 매니저들 ActiveOffAll 돌리니까,
        // 사운드는 "꺼버리면" 불편해서 아무것도 안 함.
    }

    public override void Update()
    {
        // 현재는 매 프레임 처리할 것 없음
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
        // 씬이 바뀌면 "전투/엔딩 스택"은 상황에 따라 유지/초기화 선택인데
        // 보통 전투는 같은 씬에서 발생하니까 유지해도 되고,
        // 씬 로드가 발생했다면 일단 스택은 비우는게 안전함.
        _bgmStack.Clear();

        if (_sceneToBgm.TryGetValue(scene.name, out var bgm))
            PlayBgm(bgm);
        else
            PlayBgm(_config.defaultBgm);
    }

    // -------------------------
    // Public API (다른 매니저/스크립트에서 호출)
    // -------------------------
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

        //fadeSeconds가 0이면 "즉시" 적용
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

            // next를 current로 사용
            _nextBgm.clip = clip;
            _nextBgm.loop = loop;
            _nextBgm.volume = targetTo;
            _nextBgm.Play();

            // swap
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

    // 전투/엔딩 시작: 현재 BGM 저장하고 새 BGM 재생
    public void PushBgm(BgmId id, float fadeSeconds = -1f)
    {
        if (_currentBgmId != BgmId.None)
            _bgmStack.Push(_currentBgmId);

        PlayBgm(id, fadeSeconds);
    }

    // 전투/엔딩 끝: 저장된 BGM 복귀
    public void PopBgm(float fadeSeconds = -1f)
    {
        if (_bgmStack.Count <= 0) return;
        var prev = _bgmStack.Pop();
        PlayBgm(prev, fadeSeconds);
    }

    // 인게임 씬 하나에서 "마을/던전"만 전환되는 구조면 이걸 호출해서 바꿔도 됨
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

        // swap
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