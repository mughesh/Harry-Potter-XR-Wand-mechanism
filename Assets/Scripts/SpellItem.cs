using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SpellItem : MonoBehaviour
{
    public SpellData spellData;
    private SpellSystem spellSystem;
    private Renderer spellRenderer;
    private XRSimpleInteractable interactable;

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
    }

    public void OnSpellSelected(SelectEnterEventArgs args)
    {
        if (spellSystem != null)
        {
            spellSystem.SelectSpell(spellData);
            HighlightSpell(true);
            Debug.Log("Spell selected: " + spellData.spellName);
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
            if (highlight)
            {
                spellRenderer.material.EnableKeyword("_EMISSION");
            }
            else
            {
                spellRenderer.material.DisableKeyword("_EMISSION");
            }
        }
    }
}