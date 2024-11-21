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
    private GameObject activeSpellVFX; // Track the active spell VFX instance
    private GameObject activeLineRenderer; // Track the active line renderer for ray spells
    private Dictionary<string, float> spellDurations = new Dictionary<string, float>();
    private bool isSpellActive = false;
    public bool IsSpellActive => isSpellActive;
    private GameObject levitatedObject;
    private Rigidbody levitatedRigidbody;
    private LineRenderer levitationLineRenderer;

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
        // If it's a hold-type spell and we already have an active instance, just update it
        if (spell.triggerType == SpellTriggerType.Hold && isSpellActive)
        {
            UpdateActiveSpell(spell, startPosition, direction);
            return;
        }
        else
        {
            // Otherwise, start a new spell cast
            StartCoroutine(CastSpellCoroutine(spell, startPosition, direction));
        }

    }

    private void UpdateActiveSpell(SpellData spell, Vector3 startPosition, Vector3 direction)
    {
        switch (spell.castType)
        {
            case SpellCastType.Ray:
                UpdateRaySpell(startPosition, direction);
                break;
            case SpellCastType.Utility:
                CastUtilitySpell(spell, startPosition, direction);
                break;
                // Add other cases as needed
        }
    }

    private IEnumerator CastSpellCoroutine(SpellData spell, Vector3 startPosition, Vector3 direction)
    {
        if (spell.castVFXPrefab != null)
        {
            // Clean up any existing spell VFX
            CleanupActiveSpell();

            // Create new spell instance
            activeSpellVFX = Instantiate(spell.castVFXPrefab, startPosition, Quaternion.LookRotation(direction));
            isSpellActive = true;

            switch (spell.castType)
            {
                case SpellCastType.Projectile:
                    yield return StartCoroutine(CastProjectileSpell(activeSpellVFX, startPosition, direction));
                    break;
                case SpellCastType.Ray:
                    yield return StartCoroutine(CastRaySpell(spell, startPosition, direction));
                    break;
                case SpellCastType.Area:
                    yield return StartCoroutine(CastAreaSpell(spell, startPosition));
                    break;
                case SpellCastType.Utility:
                    yield return StartCoroutine(CastUtilitySpell(spell, startPosition, direction));
                    break;
            }

            // Cleanup after spell is complete
            CleanupActiveSpell();
        }

        else
        {
            Debug.LogWarning("No cast VFX prefab assigned for the spell.");
        }
    }


    private void CleanupActiveSpell()
    {
        if (activeSpellVFX != null)
        {
            activeSpellVFX.transform.SetParent(null);
            Destroy(activeSpellVFX);
            activeSpellVFX = null;
        }
        if (activeLineRenderer != null)
        {
            Destroy(activeLineRenderer);
            activeLineRenderer = null;
        }
        isSpellActive = false;
    }

    private IEnumerator CastUtilitySpell(SpellData spell, Vector3 startPosition, Vector3 direction)
    {
        switch (spell.spellName)
        {
            case "Lumos":
                yield return StartCoroutine(CastLumos(spell, startPosition, direction));
                break;
            case "Wingardium Leviosa":
                yield return StartCoroutine(CastWingardiumLeviosa(spell, startPosition, direction));
                break;
                // Add other utility spells
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

    private IEnumerator CastRaySpell(SpellData spell, Vector3 startPosition, Vector3 direction)
    {
        // Create line renderer if it doesn't exist
        if (activeLineRenderer == null && spell.lineRendererPrefab != null)
        {
            activeLineRenderer = Instantiate(spell.lineRendererPrefab, startPosition, Quaternion.identity);
        }

        UpdateRaySpell(startPosition, direction);

        if (spell.triggerType == SpellTriggerType.Press)
        {
            yield return new WaitForSeconds(0.5f); // Duration for press-type ray spells
        }
        else
        {
            // For hold-type, keep updating until spell is deactivated
            while (isSpellActive)
            {
                yield return null;
            }
        }
    }

    private void UpdateRaySpell(Vector3 startPosition, Vector3 direction)
    {
        if (activeLineRenderer != null)
        {
            LineRenderer lineRenderer = activeLineRenderer.GetComponent<LineRenderer>();
            if (Physics.Raycast(startPosition, direction, out RaycastHit hit, maxCastDistance))
            {
                lineRenderer.SetPosition(0, startPosition);
                lineRenderer.SetPosition(1, hit.point);

                if (activeSpellVFX != null)
                {
                    activeSpellVFX.transform.position = hit.point;
                    activeSpellVFX.transform.rotation = Quaternion.LookRotation(-hit.normal);
                }
            }
            else
            {
                Vector3 endPoint = startPosition + direction * maxCastDistance;
                lineRenderer.SetPosition(0, startPosition);
                lineRenderer.SetPosition(1, endPoint);

                if (activeSpellVFX != null)
                {
                    activeSpellVFX.transform.position = endPoint;
                    activeSpellVFX.transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
    }


    private IEnumerator CastAreaSpell(SpellData spell, Vector3 startPosition)
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

    public SpellData GetSpellByName(string spellName)
    {
        foreach (SpellData spell in availableSpells)
        {
            if (spell.spellName == spellName)
            {
                return spell;
            }
        }
        return null;
    }

    public void StopActiveSpell()
    {
        isSpellActive = false;
        CleanupActiveSpell();
        ReleaseLevitatedObject();
    }

    // UTILITY SPELLS FUNCTIONS --------------

    // LUMOS
private IEnumerator CastLumos(SpellData spell, Vector3 startPosition, Vector3 direction)
{
    // Get or add a parent transform to keep the light attached to the wand
    Transform wandTip = GameObject.FindObjectOfType<WandController>().wandTip;
    if (wandTip == null)
    {
        Debug.LogError("Wand tip not found for Lumos spell!");
        yield break;
    }

    // Parent the VFX to the wand tip
    if (activeSpellVFX != null)
    {
        activeSpellVFX.transform.SetParent(wandTip, false);
        activeSpellVFX.transform.localPosition = Vector3.zero + Vector3.forward * 0.05f;
        activeSpellVFX.transform.localRotation = Quaternion.identity;

        // Light lumosLight = activeSpellVFX.GetComponentInChildren<Light>();
        // if (lumosLight != null)
        // {
        //     while (isSpellActive)
        //     {
        //         // Just animate the light intensity since position is handled by parenting
        //         float intensity = Mathf.Lerp(1f, 5f, Mathf.Sin(Time.time * 2f));
        //         lumosLight.intensity = intensity;
        //         yield return null;
        //     }
        // }
    }
}


private IEnumerator CastWingardiumLeviosa(SpellData spell, Vector3 startPosition, Vector3 direction)
{
    RaycastHit hit;
    if (Physics.Raycast(startPosition, direction, out hit, maxCastDistance))
    {
        Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            // Store the initial distance and object
            levitatedObject = hit.collider.gameObject;
            levitatedRigidbody = rb;
            float initialDistance = hit.distance;

            // Create line renderer if it doesn't exist
            if (activeLineRenderer == null && spell.lineRendererPrefab != null)
            {
                activeLineRenderer = Instantiate(spell.lineRendererPrefab, startPosition, Quaternion.identity);
                
                // Optional: Configure line renderer for a curved effect
                LineRenderer lineRenderer = activeLineRenderer.GetComponent<LineRenderer>();
                lineRenderer.positionCount = 3; // For curved line
            }

            // Disable gravity and make object kinematic
            levitatedRigidbody.useGravity = false;
            levitatedRigidbody.isKinematic = true;

            // While spell is active and trigger is held
            while (isSpellActive)
            {
                // Get current wand position and direction
                WandController wandController = FindObjectOfType<WandController>();
                if (wandController != null)
                {
                    startPosition = wandController.wandTip.position;
                    direction = wandController.wandTip.forward;

                    // Update levitation effect
                    UpdateLevitationEffect(startPosition, direction, initialDistance);
                }

                yield return null;
            }

            // Clean up levitation
            ReleaseLevitatedObject();
        }
    }
}

private void UpdateLevitationEffect(Vector3 startPosition, Vector3 direction, float initialDistance)
{
    if (activeLineRenderer != null && levitatedObject != null)
    {
        LineRenderer lineRenderer = activeLineRenderer.GetComponent<LineRenderer>();
        
        // Calculate target position at the same initial distance
        Vector3 targetPosition = startPosition + direction * initialDistance;

        // Smooth movement of the object
        levitatedObject.transform.position = Vector3.Lerp(
            levitatedObject.transform.position, 
            targetPosition, 
            Time.deltaTime * 10f
        );

        // Create a curved line effect
        Vector3 midPoint = Vector3.Lerp(startPosition, targetPosition, 0.5f);
        midPoint += Vector3.up * (initialDistance * 0.2f); // Bend downwards

        // Set line renderer positions with a curve
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, midPoint);
        lineRenderer.SetPosition(2, targetPosition);
    }
}

    private void ReleaseLevitatedObject()
    {
        if (levitatedRigidbody != null)
        {
            // Restore gravity and kinematic state
            levitatedRigidbody.useGravity = true;
            levitatedRigidbody.isKinematic = false;

            // Clear references
            levitatedObject = null;
            levitatedRigidbody = null;
        }

        // Clean up line renderer
        CleanupActiveSpell();
    }


}