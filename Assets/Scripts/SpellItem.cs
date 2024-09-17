using UnityEngine;

public class SpellItem : MonoBehaviour
{
    public SpellData spellData;

    public void OnSpellSelected()
    {
        // Logic for when the spell is selected
        Debug.Log($"Spell selected: {spellData.spellName}");
    }
}