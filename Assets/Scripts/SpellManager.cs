using UnityEngine;

public class SpellManager : MonoBehaviour
{
    public WandController wandController;

    void Start()
    {
        wandController.OnSpellSelected.AddListener(PrepareSpell);
        wandController.OnSpellCast.AddListener(CastSpell);
    }

    public void PrepareSpell(string spellName)
    {
        Debug.Log($"Preparing spell: {spellName}");
        // Here you could add visual effects to the wand, play a sound, etc.
    }

    public void CastSpell(string spellName)
    {
        switch (spellName)
        {
            case "Lumos":
                CastLumos();
                break;
            case "Reducto":
                CastReducto();
                break;
            case "Incendio":
                CastIncendio();
                break;
            default:
                Debug.LogWarning($"Unknown spell: {spellName}");
                break;
        }
    }

    public void CastLumos()
    {
        Debug.Log("Casting Lumos!");
        // Implement Lumos effect (e.g., create a light source)
    }

    public void CastReducto()
    {
        Debug.Log("Casting Reducto!");
        // Implement Reducto effect (e.g., destroy a target object)
    }

    public void CastIncendio()
    {
        Debug.Log("Casting Incendio!");
        // Implement Incendio effect (e.g., create fire particles)
    }
}