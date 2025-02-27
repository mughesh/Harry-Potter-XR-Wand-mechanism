using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SpellItem : MonoBehaviour
{
    public SpellData spellData;
    private SpellSystem spellSystem;
    private Renderer spellRenderer;
    private XRSimpleInteractable interactable;
    private Color highlightColor = Color.white;
    private float highlightIntensity = 1.5f;

    void Start()
    {
        spellSystem = FindObjectOfType<SpellSystem>();
        spellRenderer = GetComponent<Renderer>();
        interactable = GetComponent<XRSimpleInteractable>();

        if (interactable == null)
        {
            interactable = gameObject.AddComponent<XRSimpleInteractable>();
        }

        interactable.selectEntered.AddListener(OnSpellSelected);
        HighlightSpell(false);
    }

    public void OnSpellHovered(HoverEnterEventArgs args)
    {
        if (spellSystem != null)
        {
            spellSystem.HoverSpell(spellData);
            Debug.Log("Spell hovered: " + spellData.spellName);
        }
    }
    public void OnSpellSelected(SelectEnterEventArgs args)
    {
        if (spellSystem != null)
        {
            // Deselect all other spells first
            SpellItem[] allSpells = FindObjectsOfType<SpellItem>();
            foreach (SpellItem spell in allSpells)
            {
                spell.HighlightSpell(false);
            }

            spellSystem.SelectSpell(spellData);
            HighlightSpell(true);
        }
        else
        {
            Debug.LogError("SpellSystem not found in the scene.");
        }
    }
    public void HighlightSpell(bool highlight)
    {
        if (spellRenderer != null)
        {
            Material mat = spellRenderer.material;
            if (highlight)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", highlightColor * highlightIntensity);
            }
            else
            {
                mat.DisableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
            }
        }
    }
}