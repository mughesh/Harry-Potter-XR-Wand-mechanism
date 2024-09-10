using UnityEngine;

public class IncendioSpell : BaseSpell
{
    public override string SpellName => "Incendio";
    public override string Description => "Creates fire";

    private void Awake()
    {
        Debug.Log("IncendioSpell: Awake called");
        // Don't assign prefabs here, they should be assigned in the Inspector
    }

    public override void Equip()
    {
        Debug.Log("IncendioSpell: Equip called");
        base.Equip();
    }

    public override void Cast(Transform wandTip)
    {
        Debug.Log("IncendioSpell: Cast method called");
        base.Cast(wandTip);
        Debug.Log("IncendioSpell: base.Cast completed");
        // Additional Incendio-specific cast logic if needed
    }

    protected override void HandleSpellHit(RaycastHit hit)
    {
        Debug.Log("IncendioSpell: HandleSpellHit called");
        base.HandleSpellHit(hit);
        // Additional Incendio-specific hit logic
        // e.g., set the hit object on fire
        Debug.Log($"Incendio hit {hit.collider.name} at {hit.point}");
    }
}