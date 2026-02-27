using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Config/SoundManagerConfigSO")]
public class SoundManagerConfigSO : BaseScriptableObject
{
    public BgmId defaultBgm = BgmId.Lobby;

    [Header("소리 (0~1)")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Fade")]
    [Min(0f)] public float defaultFadeSeconds = 0.6f;

    [Serializable]
    public class BgmEntry
    {
        public BgmId id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop = true;
    }

    [Serializable]
    public class SfxEntry
    {
        public SfxId id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Serializable]
    public class SceneBgmEntry
    {
        [Tooltip("SceneManager의 scene.name 과 동일")]
        public string sceneName;
        public BgmId bgm;
    }

    [Header("Clip")]
    public List<BgmEntry> bgms = new();
    public List<SfxEntry> sfxs = new();

    [Header("씬별 BGM구분")]
    public List<SceneBgmEntry> sceneBgms = new();
}