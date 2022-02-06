using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{
    private static bool _initialized = false;
    private static Dictionary<string, AudioClip> _cache;

    private static AudioSource _audioSource;
    public static void Init(GameObject go)
    {
        if (_audioSource == null)
        {
            _audioSource = go.AddComponent<AudioSource>() as AudioSource;
        }

        _initialized = true;
        _cache = new Dictionary<string, AudioClip>();

        CacheClip(Consts.SOUND_RESOURCE_WALL);
        CacheClip(Consts.SOUND_RESOURCE_PADDLE);
        for (int i = 0; i < Consts.NUM_BRICK_ROWS; i++) {
            CacheClip(string.Format(Consts.SOUND_RESOURCE_BRICK_TEMPLATE, i));
        }
    }

    public static void CacheClip(string resourceName) {
        var clip = Resources.Load<AudioClip>(resourceName);
        _cache[resourceName] = clip;
    }

    public static AudioClip GetAudioClip(string resourceName)
    {
        if (!_initialized)
        {
            throw new Exception("Can't use AudioManager before it's initialzed!");
        }

        if (!_cache.ContainsKey(resourceName))
        {
            CacheClip(resourceName);
        }

        return _cache[resourceName];
    }

    public static void PlaySound(string resourceName)
    {
        var sound = GetAudioClip(resourceName);
        _audioSource.PlayOneShot(sound);
    }
}