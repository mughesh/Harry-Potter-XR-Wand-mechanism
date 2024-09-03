using UnityEngine;

public class SpellEffects : MonoBehaviour
{
    public ParticleSystem defaultEffect;
    public ParticleSystem reductoEffect;
    public ParticleSystem incendioEffect;
    // Add more effects for other spells

    public void ApplySpellEffect(string spellName)
    {
        // Disable all effects first
        defaultEffect.Stop();
        reductoEffect.Stop();
        incendioEffect.Stop();

        // Enable the appropriate effect
        switch (spellName)
        {
            case "Default":
                defaultEffect.Play();
                break;
            case "Reducto":
                reductoEffect.Play();
                break;
            case "Incendio":
                incendioEffect.Play();
                break;
            // Add more cases for other spells
            default:
                defaultEffect.Play();
                break;
        }
    }
}