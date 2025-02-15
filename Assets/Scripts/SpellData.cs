using UnityEngine;

[CreateAssetMenu(fileName = "New Spell", menuName = "Spell Data")]
public class SpellData : ScriptableObject
{
    public string spellName;
    public string description;
    public GameObject equipVFXPrefab;
    public GameObject castVFXPrefab;
    public GameObject hitVFXPrefab;
    public GameObject lineRendererPrefab;
    public SpellCastType castType;
    public SpellTriggerType triggerType;
    public float levitationBendStrength = 0.2f;
    public float levitationSmoothSpeed = 10f;
    
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
