using UnityEngine;
using System.Collections.Generic;

public class BookController : MonoBehaviour
{
    public GameObject spellsParent;
    public Material defaultMaterial;
    public Material selectedMaterial;
    public float hoverTime = 1f;

    private Dictionary<string, GameObject> spellObjects = new Dictionary<string, GameObject>();
    private float currentHoverTime = 0f;
    private GameObject hoveredSpell;

    void Start()
    {
        foreach (Transform child in spellsParent.transform)
        {
            spellObjects[child.name] = child.gameObject;
        }
    }

    public void OnSpellAimed(string spellName)
    {
        if (spellObjects.TryGetValue(spellName, out GameObject spellObject))
        {
            if (hoveredSpell != spellObject)
            {
                ResetHover();
                hoveredSpell = spellObject;
            }
            currentHoverTime += Time.deltaTime;

            if (currentHoverTime >= hoverTime)
            {
                ShowSpellDescription(spellName);
            }
        }
        else
        {
            ResetHover();
        }
    }

    public void OnSpellSelected(string spellName)
    {
        foreach (var spell in spellObjects.Values)
        {
            spell.GetComponent<Renderer>().material = defaultMaterial;
        }

        if (spellObjects.TryGetValue(spellName, out GameObject selectedSpell))
        {
            selectedSpell.GetComponent<Renderer>().material = selectedMaterial;
        }
    }

    private void ResetHover()
    {
        hoveredSpell = null;
        currentHoverTime = 0f;
        // Hide spell description
    }

    private void ShowSpellDescription(string spellName)
    {
        // Show spell description UI
        Debug.Log($"Showing description for {spellName}");
    }
}