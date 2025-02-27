using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("AudioManager");
                _instance = go.AddComponent<AudioManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        [Range(0f, 1f)]
        public float spatialBlend = 1f; // 0 = 2D, 1 = 3D
        public float minDistance = 1f;
        public float maxDistance = 25f;
        public bool loop = false;
    }

    // Sound categories
    [Header("Spell Sounds")]
    public SoundEffect spellEquip;
    public SoundEffect incendioCast;
    public SoundEffect incendioHit;
    public SoundEffect reductoCast;
    public SoundEffect reductoHit;
    public SoundEffect leviosaActive;
    public SoundEffect lumos;

    [Header("Item Sounds")]
    public SoundEffect equipBook;
    public SoundEffect equipWand;
    public SoundEffect pageTurn;
    public SoundEffect broomFlying;
    public SoundEffect itemCollision;

    // Pool of audio sources for 3D sounds
    private List<AudioSource> audioSourcePool = new List<AudioSource>();
    private int poolSize = 10;
    private Transform audioSourceParent;

    // Dictionary to track looping sounds
    private Dictionary<string, AudioSource> activeLoopingSounds = new Dictionary<string, AudioSource>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Create a parent for all pooled audio sources
        audioSourceParent = new GameObject("AudioSources").transform;
        audioSourceParent.SetParent(transform);

        // Initialize audio source pool
        for (int i = 0; i < poolSize; i++)
        {
            CreateAudioSource();
        }
    }

    private AudioSource CreateAudioSource()
    {
        GameObject audioObj = new GameObject("AudioSource_" + audioSourcePool.Count);
        audioObj.transform.SetParent(audioSourceParent);
        
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSourcePool.Add(audioSource);
        
        return audioSource;
    }

    private AudioSource GetAvailableAudioSource()
    {
        // Find an available audio source in the pool
        foreach (var source in audioSourcePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // If no available source is found, create a new one and add it to the pool
        return CreateAudioSource();
    }

    // Play a one-shot sound at a position in 3D space
    public AudioSource PlaySoundAtPosition(SoundEffect sound, Vector3 position)
    {
        if (sound == null || sound.clip == null)
        {
            Debug.LogWarning("Attempted to play a null sound");
            return null;
        }

        AudioSource audioSource = GetAvailableAudioSource();
        audioSource.clip = sound.clip;
        audioSource.volume = sound.volume;
        audioSource.pitch = sound.pitch;
        audioSource.spatialBlend = sound.spatialBlend;
        audioSource.minDistance = sound.minDistance;
        audioSource.maxDistance = sound.maxDistance;
        audioSource.loop = sound.loop;
        audioSource.transform.position = position;
        audioSource.Play();

        // If this is a looping sound, track it
        if (sound.loop)
        {
            string key = sound.name + "_" + position.ToString();
            activeLoopingSounds[key] = audioSource;
        }

        return audioSource;
    }

    // Play a one-shot sound following a transform
    public AudioSource PlaySoundFollowingTransform(SoundEffect sound, Transform target)
    {
        if (sound == null || sound.clip == null || target == null)
        {
            Debug.LogWarning("Attempted to play a null sound or on a null target");
            return null;
        }

        AudioSource audioSource = GetAvailableAudioSource();
        audioSource.clip = sound.clip;
        audioSource.volume = sound.volume;
        audioSource.pitch = sound.pitch;
        audioSource.spatialBlend = sound.spatialBlend;
        audioSource.minDistance = sound.minDistance;
        audioSource.maxDistance = sound.maxDistance;
        audioSource.loop = sound.loop;
        
        // Parent to the target transform to follow it
        audioSource.transform.SetParent(target);
        audioSource.transform.localPosition = Vector3.zero;
        audioSource.Play();

        // If this is a looping sound, track it
        if (sound.loop)
        {
            string key = sound.name + "_" + target.name;
            activeLoopingSounds[key] = audioSource;
            return audioSource;
        }
        else
        {
            // For non-looping sounds, detach from parent after playing
            StartCoroutine(DetachAfterPlaying(audioSource));
            return audioSource;
        }
    }

    // Stop a looping sound
    public void StopLoopingSound(string soundName, Transform target)
    {
        string key = soundName + "_" + target.name;
        if (activeLoopingSounds.TryGetValue(key, out AudioSource source))
        {
            source.Stop();
            source.transform.SetParent(audioSourceParent);
            source.transform.localPosition = Vector3.zero;
            activeLoopingSounds.Remove(key);
        }
    }

    // Stop a looping sound at a position
    public void StopLoopingSound(string soundName, Vector3 position)
    {
        string key = soundName + "_" + position.ToString();
        if (activeLoopingSounds.TryGetValue(key, out AudioSource source))
        {
            source.Stop();
            activeLoopingSounds.Remove(key);
        }
    }

    private IEnumerator DetachAfterPlaying(AudioSource audioSource)
    {
        // Wait for the clip to finish playing
        float clipLength = audioSource.clip.length;
        yield return new WaitForSeconds(clipLength);

        // Return to the pool parent
        if (audioSource != null)
        {
            audioSource.transform.SetParent(audioSourceParent);
            audioSource.transform.localPosition = Vector3.zero;
        }
    }
}