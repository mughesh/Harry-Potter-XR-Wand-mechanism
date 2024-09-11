using UnityEngine;

public class BookController : MonoBehaviour
{
    public float hoverTime = 1f;

    private SpellManager spellManager;
    private GameObject currentHoveredSpell;
    private float hoverStartTime;

    void Start()
    {
        spellManager = FindObjectOfType<SpellManager>();
    }

    public void HoverSpell(GameObject spellObject)
    {
        if (currentHoveredSpell != spellObject)
        {
            currentHoveredSpell = spellObject;
            hoverStartTime = Time.time;
        }

        if (Time.time - hoverStartTime >= hoverTime)
        {
            ShowSpellDescription(spellObject.GetComponent<BaseSpell>());
        }
    }

    public void SelectSpell(GameObject spellObject)
    {
        BaseSpell spell = spellObject.GetComponent<BaseSpell>();
        if (spell != null)
        {
            spellManager.SelectSpell(spell.spellName);
        }
    }

    private void ShowSpellDescription(BaseSpell spell)
    {
        // Implement spell description UI logic here
        Debug.Log($"Showing description for {spell.spellName}");
    }

    public void UpdateSpellVisuals(string selectedSpellName)
    {
        foreach (BaseSpell spell in GetComponentsInChildren<BaseSpell>())
        {
            Material spellMaterial = spell.spellMaterial;
            if (spell.spellName == selectedSpellName)
            {
                spellMaterial.EnableKeyword("_EMISSION");
                spellMaterial.SetColor("_EmissionColor", Color.white);
            }
            else
            {
                spellMaterial.DisableKeyword("_EMISSION");
            }
            spell.GetComponent<Renderer>().material = spellMaterial;
        }
    }

    public void ResetSpellVisuals()
    {
        foreach (BaseSpell spell in GetComponentsInChildren<BaseSpell>())
        {
            Material spellMaterial = spell.spellMaterial;
            spellMaterial.DisableKeyword("_EMISSION");
            spell.GetComponent<Renderer>().material = spellMaterial;
        }
    }
}