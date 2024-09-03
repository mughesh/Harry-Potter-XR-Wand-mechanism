using UnityEngine;
using System.Collections;

public class SpellManager : MonoBehaviour
{
    public WandController wandController;
    public GameObject lumousFx;

    private Coroutine lumosCoroutine;

    void Start()
    {
        wandController.OnSpellSelected.AddListener(PrepareSpell);
        wandController.OnTriggerPressed.AddListener(CastSpell);
        wandController.OnTriggerHeld.AddListener(HoldSpell);
        wandController.OnTriggerReleased.AddListener(ReleaseSpell);
    }

    public void PrepareSpell(string spellName)
    {
        Debug.Log($"Preparing spell: {spellName}");
        // Add visual effects to the wand, play a sound, etc.
    }

    public void CastSpell(string spellName)
    {
        switch (spellName)
        {
            case "Lumos":
                if (lumosCoroutine == null)
                {
                    lumosCoroutine = StartCoroutine(LumosEffect());
                }
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

    public void HoldSpell(string spellName)
    {
        if (spellName == "Lumos")
        {
            // Keep the Lumos light active
            lumousFx.SetActive(true);
        }
    }

    public void ReleaseSpell(string spellName)
    {
        if (spellName == "Lumos")
        {
            if (lumosCoroutine != null)
            {
                StopCoroutine(lumosCoroutine);
                lumosCoroutine = null;
            }
            lumousFx.SetActive(false);
        }
    }

    private IEnumerator LumosEffect()
    {
        lumousFx.SetActive(true);

        // Scaling effect (or any other effect you want)
        float duration = 0.5f;
        Vector3 initialScale = Vector3.zero;
        Vector3 targetScale = new Vector3(0.03f, 0.03f, 0.03f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            lumousFx.transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        lumousFx.transform.localScale = targetScale;
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
