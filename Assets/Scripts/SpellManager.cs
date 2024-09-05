using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellManager : MonoBehaviour
{
    public WandController wandController;
    public List<GameObject> spellProjectiles;
    private GameObject activeProjectile;
    public GameObject lumousFx;
    private Coroutine lumosCoroutine;

    void Start()
    {
        wandController.OnSpellSelected.AddListener(PrepareSpell);
        wandController.OnTriggerPressed.AddListener(CastSpell);
        wandController.OnTriggerHeld.AddListener(HoldSpell);
        wandController.OnTriggerReleased.AddListener(ReleaseSpell);
        activeProjectile = spellProjectiles[wandController.currentSpell];
    }

    public void PrepareSpell(string spellName)
    {
        Debug.Log($"Preparing spell: {spellName}");
        // Add visual effects to the wand, play a sound, etc.
    }

    public void CastSpell(string spellName)
    {
        Debug.Log($"Casting spell: {spellName}");
        switch (spellName)
        {
            case "Default":
                LaunchSpellProjectile(spellName);
                Debug.Log("Casting primary spell");
                break;

            case "Lumos":
                if (lumosCoroutine == null)
                {
                    lumosCoroutine = StartCoroutine(LumosEffect());
                }
                Debug.Log("Casting Lumos spell");
                break;

            case "Reducto":
                LaunchSpellProjectile(spellName);
                Debug.Log("Casting Reducto spell");
                break;

            case "Incendio":
                LaunchSpellProjectile(spellName);
                Debug.Log("Casting Incendio spell");
                break;

            default:
                LaunchSpellProjectile(spellName);
                Debug.Log("Casting default spell");
                break;
        }
    }

    private void LaunchSpellProjectile(string spellName)
    {
        GameObject projectileObj = Instantiate(baseSpellProjectilePrefab, wandController.spawnPoint.position, wandController.spawnPoint.rotation);
        SpellProjectile projectile = projectileObj.GetComponent<SpellProjectile>();
        SpellEffects spellEffects = projectileObj.GetComponent<SpellEffects>();
        if (projectile != null)
        {
            projectile.Initialize(wandController.spawnPoint);
            Debug.Log($"Launched spell: {spellName}");
            projectile.OnSpellHit += HandleSpellHit;
        }
        if (spellEffects != null)
        {
            spellEffects.ApplySpellEffect(spellName);
        }
    }

    private void HandleSpellHit(Collision collision)
    {
        // Handle spell-specific effects on hit
        switch (wandController.currentSpell)
        {
            case "Default":
                // Handle default spell hit
                break;
            case "Reducto":
                // Handle Reducto hit effect
                break;
            case "Incendio":
                // Handle Incendio hit effect
                break;
            // Add more cases for other spells
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
            Debug.Log("Lumos spell released");
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
}