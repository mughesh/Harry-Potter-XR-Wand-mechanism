using UnityEngine;
using System.Collections.Generic;

public class SpellManager : MonoBehaviour
{
    public WandController wandController;
    public BookController bookController;

    private Dictionary<string, BaseSpell> spells = new Dictionary<string, BaseSpell>();
    private BaseSpell currentSpell;
    private GameObject aimedObject;

    void Start()
    {
        InitializeSpells();
    }

    private void InitializeSpells()
    {
        foreach (BaseSpell spell in bookController.GetComponentsInChildren<BaseSpell>())
        {
            spells.Add(spell.spellName, spell);
        }
    }

    public void AimAt(GameObject obj)
    {
        aimedObject = obj;
        if (obj != null && obj.GetComponent<BaseSpell>() != null)
        {
            bookController.HoverSpell(obj);
        }
    }

    public void SelectSpell(string spellName)
    {
        if (currentSpell != null)
        {
            currentSpell.OnDeselect();
        }

        if (spells.TryGetValue(spellName, out BaseSpell spell))
        {
            currentSpell = spell;
            currentSpell.OnSelect(wandController.spawnPoint);
            bookController.UpdateSpellVisuals(spellName);
        }
    }

    public void TriggerPressed()
    {
        if (aimedObject != null && aimedObject.GetComponent<BaseSpell>() != null)
        {
            SelectSpell(aimedObject.GetComponent<BaseSpell>().spellName);
        }
        else if (currentSpell != null)
        {
            currentSpell.OnCast(wandController.spawnPoint);
        }
    }

    public void TriggerReleased()
    {
        if (currentSpell != null)
        {
            currentSpell.OnRelease();
        }
    }

    public void WandGrabbed()
    {
        if (currentSpell != null)
        {
            currentSpell.OnSelect(wandController.spawnPoint);
        }
    }

    public void WandReleased()
    {
        if (currentSpell != null)
        {
            currentSpell.OnDeselect();
        }
        bookController.ResetSpellVisuals();
    }
}