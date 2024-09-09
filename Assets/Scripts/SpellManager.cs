using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class SpellManager : MonoBehaviour
{
    public WandController wandController;
    public Dictionary<string, GameObject> spellProjectiles;
    public List<GameObject> spellProjectileList;
    private Coroutine lumosCoroutine;
    private GameObject activeLumosFx;
    public GameObject spellDescriptionPrefab;
    private GameObject activeSpellDescription;

    void Start()
    {   
        spellProjectiles = new Dictionary<string, GameObject>
        {
            {"Default", spellProjectileList[0]},    
            {"Lumos", spellProjectileList[1]},
            {"Reducto", spellProjectileList[2]},
            {"Incendio", spellProjectileList[3]}
        };
        wandController.OnSpellAimed.AddListener(ShowSpellDescription);
        wandController.OnSpellSelected.AddListener(PrepareSpell);
        wandController.OnTriggerPressed.AddListener(CastSpell);
        wandController.OnTriggerHeld.AddListener(HoldSpell);
        wandController.OnTriggerReleased.AddListener(ReleaseSpell);
    }

    public void ShowSpellDescription(string spellName)
    {
        if (activeSpellDescription != null)
        {
            Destroy(activeSpellDescription);
        }

        if (!string.IsNullOrEmpty(spellName))
        {
            activeSpellDescription = Instantiate(spellDescriptionPrefab, wandController.transform.position + wandController.transform.up * 0.2f, Quaternion.identity);
            activeSpellDescription.GetComponent<TextMesh>().text = GetSpellDescription(spellName);
        }
    }

    private string GetSpellDescription(string spellName)
    {
        switch (spellName)
        {
            case "Lumos": 
                return "Creates light";
                
            case "Reducto": 
                return "Breaks objects";

            case "Incendio": 
                return "Creates fire";

            default: 
                return "Unknown spell";
        }
    }

    public void PrepareSpell(string spellName)
    {
        Debug.Log($"Preparing spell: {spellName}");
        // Add visual effects to the wand, play a sound, etc.
    }

   public void CastSpell(string spellName)
    {
        Debug.Log($"Casting spell: {spellName}");
        if (spellProjectiles.TryGetValue(spellName, out GameObject spellProjectile))
        {
            if (spellName == "Lumos")
            {
                if (lumosCoroutine == null)
                {
                    lumosCoroutine = StartCoroutine(LumosEffect(spellProjectile));
                }
                              
            }
            else
            {
                LaunchSpellProjectile(spellName, spellProjectile);
                //ResetToDefaultSpell();
            }
        }
        else
        {
            Debug.LogWarning($"Spell {spellName} not found in dictionary. Casting default spell.");
            LaunchSpellProjectile("Default", spellProjectiles["Default"]);
        }
    }

    

    private void LaunchSpellProjectile(string spellName, GameObject projectilePrefab)
    {
        GameObject projectileObj = Instantiate(projectilePrefab, wandController.spawnPoint.position, wandController.spawnPoint.rotation);
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
        string currentSpell = wandController.currentSpell;
        if (spellProjectiles.ContainsKey(currentSpell))
        {
            switch (currentSpell)
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
                // Add more cases for other spells if needed
            }
        }
        else
        {
            Debug.LogWarning($"Spell {currentSpell} not found in dictionary during hit handling.");
        }
    }

    public void HoldSpell(string spellName)
    {
        if (spellName == "Lumos" && activeLumosFx != null)
        {
            // Keep the Lumos light active
            activeLumosFx.SetActive(true);
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
                ResetToDefaultSpell();
            }
            if (activeLumosFx != null)
            {
                Destroy(activeLumosFx);
                activeLumosFx = null;
            }
        }
    }

    private void ResetToDefaultSpell()
    {
        wandController.currentSpell = "Default";
        wandController.OnSpellSelected.Invoke("Default");
        Debug.Log("Reset to default spell after casting.");
    }

    private IEnumerator LumosEffect(GameObject lumosPrefab)
    {
        // Instantiate the Lumos effect at the spawn point
        activeLumosFx = Instantiate(lumosPrefab, wandController.spawnPoint.position, wandController.spawnPoint.rotation, wandController.spawnPoint);
        activeLumosFx.SetActive(true);

        // Scaling effect
        float duration = 0.5f;
        Vector3 initialScale = Vector3.zero;
        Vector3 targetScale = new Vector3(0.03f, 0.03f, 0.03f);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            activeLumosFx.transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        activeLumosFx.transform.localScale = targetScale;
    }
}