using UnityEngine;

[CreateAssetMenu(fileName = "New Spell", menuName = "Spell Data")]
public class SpellData : ScriptableObject
{
    public string spellName;
    public string description;
    public GameObject equipVFXPrefab;
    public GameObject castVFXPrefab;  // For state spells, this will be the persistent effect prefab
    public GameObject hitVFXPrefab;
    public GameObject lineRendererPrefab;
    public SpellCastType castType;
    public SpellTriggerType triggerType;
    
    // Optional settings for state spells (like Lumos)
    public float effectDuration = -1f;  // -1 means persist until cancelled
    public bool canBeToggled = true;    // Whether the spell can be turned off by casting again
}

public enum SpellCastType
{
    Projectile,
    Ray,
    Area,
    State,
    Buff
}

public enum SpellTriggerType
{
    Press,
    Hold
}