using UnityEngine;
using System.Collections;

public class SpellSystem : MonoBehaviour
{
    private SpellData currentSpell;

    public SpellData CurrentSpell => currentSpell;

    public void SelectSpell(SpellData spell)
    {
        currentSpell = spell;
        Debug.Log($"Selected spell: {currentSpell.spellName}");
        EquipSpell();
    }

    private void EquipSpell()
    {
        if (currentSpell != null && currentSpell.equipVFXPrefab != null)
        {
            GameObject equipVFX = Instantiate(currentSpell.equipVFXPrefab, transform.position, Quaternion.identity);
            Destroy(equipVFX, 2f); // Destroy the VFX after 2 seconds
        }
    }

    public void CastSpell(Vector3 startPosition, Vector3 direction)
    {
        if (currentSpell != null)
        {
            Debug.Log($"Casting spell: {currentSpell.spellName}");
            StartCoroutine(CastSpellCoroutine(startPosition, direction));
        }
    }

    private IEnumerator CastSpellCoroutine(Vector3 startPosition, Vector3 direction)
    {
        if (currentSpell.castVFXPrefab != null)
        {
            GameObject castVFX = Instantiate(currentSpell.castVFXPrefab, startPosition, Quaternion.LookRotation(direction));
            
            // Handle different spell cast types
            switch (currentSpell.castType)
            {
                case SpellCastType.Projectile:
                    yield return StartCoroutine(ProjectileSpell(castVFX, startPosition, direction));
                    break;
                case SpellCastType.Ray:
                    yield return StartCoroutine(RaySpell(castVFX, startPosition, direction));
                    break;
                case SpellCastType.Area:
                    yield return StartCoroutine(AreaSpell(castVFX, startPosition));
                    break;
            }

            Destroy(castVFX);
        }
    }

    private IEnumerator ProjectileSpell(GameObject castVFX, Vector3 startPosition, Vector3 direction)
    {
        float speed = 10f;
        float maxDistance = 20f;
        float distanceTraveled = 0f;

        while (distanceTraveled < maxDistance)
        {
            castVFX.transform.position += direction * speed * Time.deltaTime;
            distanceTraveled += speed * Time.deltaTime;

            if (Physics.Raycast(castVFX.transform.position, direction, out RaycastHit hit, 0.5f))
            {
                SpellHitEffect(hit.point, hit.normal);
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator RaySpell(GameObject castVFX, Vector3 startPosition, Vector3 direction)
    {
        if (Physics.Raycast(startPosition, direction, out RaycastHit hit))
        {
            castVFX.transform.position = hit.point;
            SpellHitEffect(hit.point, hit.normal);
        }
        else
        {
            castVFX.transform.position = startPosition + direction * 20f;
        }

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator AreaSpell(GameObject castVFX, Vector3 startPosition)
    {
        float radius = 5f;
        float duration = 2f;
        float startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            Collider[] hitColliders = Physics.OverlapSphere(startPosition, radius);
            foreach (Collider hitCollider in hitColliders)
            {
                // Apply area effect to each object in range
                // You can add specific logic here based on the spell
            }
            yield return null;
        }
    }

    private void SpellHitEffect(Vector3 position, Vector3 normal)
    {
        if (currentSpell.hitVFXPrefab != null)
        {
            GameObject hitVFX = Instantiate(currentSpell.hitVFXPrefab, position, Quaternion.LookRotation(normal));
            Destroy(hitVFX, 2f);
        }
    }
}