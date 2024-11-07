using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class SpellSystem : MonoBehaviour
{
    [SerializeField] private SpellData[] availableSpells;
    private SpellData currentSpell;
    public float maxCastDistance = 20f;

    public SpellData CurrentSpell => currentSpell;

    private void Start()
    {
        // // Initialize with the first spell if available
        // if (availableSpells.Length > 0)
        // {
        //     SelectSpell(availableSpells[0]);
        // }
        //SelectSpell(availableSpells[0]);
    }

    public void SelectSpell(SpellData spell)
    {
        currentSpell = spell;
        Debug.Log($"Selected spell: {currentSpell.spellName}");
        EquipSpell();
    }

    public void HoverSpell(SpellData spell)
    {
        if (currentSpell == spell)
        {
            Debug.Log($"Hovering spell: {currentSpell.description}");
        }
    }

    private void EquipSpell()
    {
        if (currentSpell != null && currentSpell.equipVFXPrefab != null)
        {
            GameObject equipVFX = Instantiate(currentSpell.equipVFXPrefab, transform.position, Quaternion.identity);
            Destroy(equipVFX, 2f);
        }
    }

    public void ResetSpell()
    {
        currentSpell = null;
        Debug.Log("Spell reset");
    }

    public void CastSpell(SpellData spell, Vector3 startPosition, Vector3 direction)
    {
        if (spell != null)
        {
            Debug.Log($"Casting spell: {spell.spellName}");
            StartCoroutine(CastSpellCoroutine(spell, startPosition, direction));
        }
        else
        {
            Debug.LogWarning("No spell selected!");
        }
    }

    private IEnumerator CastSpellCoroutine(SpellData spell, Vector3 startPosition, Vector3 direction)
    {
        if (spell.castVFXPrefab != null)
        {
            GameObject castVFX = Instantiate(spell.castVFXPrefab, startPosition, Quaternion.LookRotation(direction));

            switch (spell.castType)
            {
                case SpellCastType.Projectile:
                    yield return StartCoroutine(CastProjectileSpell(castVFX, startPosition, direction));
                    break;
                case SpellCastType.Ray:
                    yield return StartCoroutine(CastRaySpell(castVFX, startPosition, direction));
                    break;
                case SpellCastType.Area:
                    yield return StartCoroutine(CastAreaSpell(castVFX, startPosition));
                    break;
            }

            Destroy(castVFX);
        }
    }

    private IEnumerator CastProjectileSpell(GameObject castVFX, Vector3 startPosition, Vector3 direction)
    {
        float speed = 10f;
        float distanceTraveled = 0f;

        // Use the same raycast as the crosshair
        RaycastHit hit;
        Vector3 targetPosition;
        if (Physics.Raycast(startPosition, direction, out hit, maxCastDistance))
        {
            targetPosition = hit.point;
        }
        else
        {
            targetPosition = startPosition + direction * maxCastDistance;
        }

        Vector3 controlPoint = startPosition + Vector3.up * 2f + direction * 5f + Random.insideUnitSphere * 2f;
        List<Vector3> path = GenerateCurvedPath(startPosition, targetPosition, controlPoint, 50);

        int pathIndex = 0;
        while (pathIndex < path.Count)
        {
            castVFX.transform.position = path[pathIndex];
            castVFX.transform.forward = (pathIndex < path.Count - 1) ? (path[pathIndex + 1] - path[pathIndex]).normalized : direction;

            if (Vector3.Distance(castVFX.transform.position, targetPosition) < 0.1f)
            {
                SpellHitEffect(targetPosition, -direction);
                yield break;
            }

            pathIndex++;
            yield return null;
        }
    }

    private IEnumerator CastRaySpell(GameObject castVFX, Vector3 startPosition, Vector3 direction)
    {
        if (Physics.Raycast(startPosition, direction, out RaycastHit hit, maxCastDistance))
        {
            castVFX.transform.position = hit.point;
            SpellHitEffect(hit.point, hit.normal);
        }
        else
        {
            castVFX.transform.position = startPosition + direction * maxCastDistance;
        }

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator CastAreaSpell(GameObject castVFX, Vector3 startPosition)
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

    private List<Vector3> GenerateCurvedPath(Vector3 start, Vector3 end, Vector3 control, int resolution)
    {
        List<Vector3> path = new List<Vector3>();
        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 point = CalculateBezierPoint(start, control, end, t);
            path.Add(point);
        }
        return path;
    }

    private Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0 + 2 * u * t * p1 + tt * p2;
        return p;
    }
}