using UnityEngine;
using System.Collections.Generic;

public class SpellManager : MonoBehaviour
{
    public WandController wandController;
    public BookController bookController;

    private Dictionary<string, BaseSpell> spells = new Dictionary<string, BaseSpell>();
    private BaseSpell currentSpell;

    void Start()
    {
        Debug.Log("SpellManager: Start method called");
        InitializeSpells();

        wandController.OnSpellAimed += HandleSpellAimed;
        wandController.OnSpellSelected += HandleSpellSelected;
        wandController.OnTriggerPressed += HandleTriggerPressed;
        wandController.OnTriggerHeld += HandleTriggerHeld;
        wandController.OnTriggerReleased += HandleTriggerReleased;
    }

    private void InitializeSpells()
    {
        Debug.Log("SpellManager: InitializeSpells called");
        //spells["Lumos"] = gameObject.AddComponent<LumosSpell>();
        //spells["Reducto"] = gameObject.AddComponent<ReductoSpell>();
        spells["Incendio"] = gameObject.AddComponent<IncendioSpell>();
        Debug.Log($"SpellManager: Incendio spell added. Total spells: {spells.Count}");
        // Add more spells here
    }

    private void HandleSpellAimed(string spellName)
    {
        Debug.Log($"SpellManager: HandleSpellAimed called with {spellName}");
        bookController.OnSpellAimed(spellName);
    }

    private void HandleSpellSelected(string spellName)
    {
        Debug.Log($"SpellManager: HandleSpellSelected called with {spellName}");
        if (spells.TryGetValue(spellName, out BaseSpell spell))
        {
            currentSpell = spell;
            currentSpell.Equip();
            bookController.OnSpellSelected(spellName);
        }
        else
        {
            Debug.LogWarning($"SpellManager: Spell {spellName} not found in dictionary");
        }
    }

    private void HandleTriggerPressed(string spellName)
    {
        Debug.Log($"SpellManager: HandleTriggerPressed called with {spellName}");
        if (currentSpell != null)
        {
            currentSpell.Cast(wandController.spawnPoint);
        }
        else
        {
            Debug.LogWarning("SpellManager: No current spell selected");
        }
    }

    private void HandleTriggerHeld(string spellName)
    {
        currentSpell?.Hold();
    }

    private void HandleTriggerReleased(string spellName)
    {
        currentSpell?.Release();
    }
}