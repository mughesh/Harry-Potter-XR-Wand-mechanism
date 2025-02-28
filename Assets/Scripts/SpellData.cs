using UnityEngine;

[CreateAssetMenu(fileName = "New Spell", menuName = "Spell Data")]
public class SpellData : ScriptableObject
{
    [Header("Basic Information")]
    public string spellName;
    public string description;

    [Header("Visual Effects")]
    public GameObject equipVFXPrefab;
    public GameObject castVFXPrefab;
    public GameObject hitVFXPrefab;
    public GameObject lineRendererPrefab;    
    
    [Header("Spell Properties")]
    public SpellCastType castType;
    public SpellTriggerType triggerType;
    public float levitationBendStrength = 0.2f;
    public float levitationSmoothSpeed = 10f;

    [Header("Flamethrower Settings")]
    public float particleSpeedMultiplier = 1f;
    public float maxParticleSpeedMultiplier = 2f;
    public float fadeOutTime = 0.01f;    
    
    [Header("Audio")]
    public AudioClip castSoundClip;
    public AudioClip hitSoundClip;
    public bool loopCastSound = false;
    [Range(0f, 1f)]
    public float castVolume = 1f;
    [Range(0f, 1f)]
    public float hitVolume = 1f;
    
}

public enum SpellCastType
{
    Projectile,
    Ray,
    Area,
    Utility
}

public enum SpellTriggerType
{
    Press,
    Hold
}
