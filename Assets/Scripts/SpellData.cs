using UnityEngine;

[CreateAssetMenu(fileName = "New Spell", menuName = "Spell Data")]
public class SpellData : ScriptableObject
{
    public string spellName;
    public string description;
    public GameObject equipVFXPrefab;
    public GameObject castVFXPrefab;
    public GameObject hitVFXPrefab;
    public SpellCastType castType;
    public SpellTriggerType triggerType;
}

public enum SpellCastType
{
    Projectile,
    Ray,
    Area
}

public enum SpellTriggerType
{
    Press,
    Hold
}