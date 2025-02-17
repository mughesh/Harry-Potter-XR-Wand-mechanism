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
    [Header("Projectile Path Controls")]
    [SerializeField] private float basePathHeight = 2f; // Base height of the curve
    [SerializeField][Range(0.1f, 1f)] private float distanceHeightRatio = 0.3f; // How much distance affects height
    [SerializeField][Range(0.1f, 1f)] private float minCurveHeight = 0.2f; // Minimum curve height for close targets
    [SerializeField] private float projectileSpeed = 10f; // Configurable projectile speed

    [SerializeField] private int curveResolution = 20; // Controls how smooth the curve looks
    [SerializeField] private float curvatureAmount = 0.5f;


    // New fields for momentum calculation
    private Vector3[] previousWandPositions;
    private int positionCount = 5;
    private int currentPositionIndex = 0;
    [SerializeField] private float releaseForceMultiplier = 3f; // Reduced from 5 for more controlled throws
    [SerializeField] private float maxThrowVelocity = 7f; // Maximum velocity cap for throws
    private Transform currentWandTip;

    private bool isProjectileInFlight = false;


    private void Start()
    {
        // // Initialize with the first spell if available
        // if (availableSpells.Length > 0)
        // {
        //     SelectSpell(availableSpells[0]);
        // }
        //SelectSpell(availableSpells[0]);

        previousWandPositions = new Vector3[positionCount];
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
        // For press-type spells, prevent multiple casts while projectile is in flight
        if (spell.triggerType == SpellTriggerType.Press)
        {
            if (isProjectileInFlight) return;
            isProjectileInFlight = true;
        }

        // If it's a hold-type spell and we already have an active instance, just update it
        if (spell.triggerType == SpellTriggerType.Hold && isSpellActive)
        {
            UpdateActiveSpell(spell, startPosition, direction);
            return;
        }

        StartCoroutine(CastSpellCoroutine(spell, startPosition, direction));
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
            Debug.Log("instantiating cast vfx");
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
                                    Debug.Log("Utility spell");
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
            // Detach from parent before destroying
            activeSpellVFX.transform.SetParent(null);
            Destroy(activeSpellVFX);
            activeSpellVFX = null;
        }

        if (activeLineRenderer != null)
        {
            Destroy(activeLineRenderer);
            activeLineRenderer = null;
        }

        // Reset levitation-specific state
        levitatedObject = null;
        levitatedRigidbody = null;
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
        try
        {
            float distanceTraveled = 0f;

            // Use the same raycast as the crosshair
            RaycastHit hit;
            Vector3 targetPosition;
            float targetDistance;

            if (Physics.Raycast(startPosition, direction, out hit, maxCastDistance))
            {
                targetPosition = hit.point;
                targetDistance = hit.distance;
            }
            else
            {
                targetPosition = startPosition + direction * maxCastDistance;
                targetDistance = maxCastDistance;
            }

            // Calculate adaptive curve height based on distance
            float adaptiveHeight = Mathf.Max(
                minCurveHeight,
                basePathHeight * (targetDistance * distanceHeightRatio)
            );

            // Calculate control point with adaptive height
            Vector3 midPoint = (startPosition + targetPosition) / 2f;
            Vector3 controlPoint = midPoint + Vector3.up * adaptiveHeight;

            // Add slight randomness to control point for natural feel
            controlPoint += Random.insideUnitSphere * 0.5f;

            List<Vector3> path = GenerateCurvedPath(startPosition, targetPosition, controlPoint, 50);

            int pathIndex = 0;
            bool hitTarget = false;

            while (pathIndex < path.Count && !hitTarget)
            {
                castVFX.transform.position = path[pathIndex];

                // Update rotation to face travel direction
                if (pathIndex < path.Count - 1)
                {
                    Vector3 nextPosition = path[pathIndex + 1];
                    Vector3 moveDirection = (nextPosition - castVFX.transform.position).normalized;
                    castVFX.transform.forward = moveDirection;
                }

                // Check for collision along the path
                if (Physics.Raycast(castVFX.transform.position, castVFX.transform.forward, out hit, 0.5f))
                {
                    SpellHitEffect(hit.point, -hit.normal);
                    hitTarget = true;
                }

                pathIndex++;

                // Add time-based movement for consistent speed
                yield return new WaitForSeconds(1f / (projectileSpeed * path.Count));
            }

            if (!hitTarget)
            {
                // Handle case where projectile reaches end without hitting
                SpellHitEffect(targetPosition, -direction);
            }

        }
        finally
        {
            isProjectileInFlight = false;
            //CleanupActiveSpell();
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
        //ReleaseLevitatedObject(Vector3.zero);
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
                levitatedObject = hit.collider.gameObject;
                levitatedRigidbody = rb;
                float initialDistance = hit.distance;

                // Store reference to wand tip for tracking
                WandController wandController = FindObjectOfType<WandController>();
                currentWandTip = wandController.wandTip;

                // Initialize position tracking array
                for (int i = 0; i < positionCount; i++)
                {
                    previousWandPositions[i] = currentWandTip.position;
                }

                if (activeLineRenderer == null && spell.lineRendererPrefab != null)
                {
                    activeLineRenderer = Instantiate(spell.lineRendererPrefab, startPosition, Quaternion.identity);
                    LineRenderer lineRenderer = activeLineRenderer.GetComponent<LineRenderer>();
                    lineRenderer.positionCount = curveResolution;
                }

                levitatedRigidbody.useGravity = false;
                levitatedRigidbody.isKinematic = true;

                while (isSpellActive)
                {
                    // Update position history
                    previousWandPositions[currentPositionIndex] = currentWandTip.position;
                    currentPositionIndex = (currentPositionIndex + 1) % positionCount;

                    if (wandController != null)
                    {
                        startPosition = wandController.wandTip.position;
                        direction = wandController.wandTip.forward;
                        UpdateLevitationEffect(startPosition, direction, initialDistance);
                    }

                    yield return null;
                }

                // Calculate and apply release velocity when spell ends
                Vector3 releaseVelocity = CalculateReleaseVelocity();
                ReleaseLevitatedObject(releaseVelocity);
            }
        }
    }

    private void UpdateLevitationEffect(Vector3 startPosition, Vector3 direction, float initialDistance)
    {
        if (activeLineRenderer != null && levitatedObject != null)
        {
            LineRenderer lineRenderer = activeLineRenderer.GetComponent<LineRenderer>();
            Vector3 targetPosition = levitatedObject.transform.position;

            lineRenderer.positionCount = curveResolution;

            // Calculate the midpoint between start and target
            Vector3 midPoint = Vector3.Lerp(startPosition, targetPosition, 0.5f);
            
            // Add downward displacement - Note the negative value for downward curve
            float downwardDisplacement = initialDistance * curvatureAmount;
            midPoint += Vector3.up * Mathf.Abs(downwardDisplacement); // Ensure downward direction


            // Generate curve points
            for (int i = 0; i < curveResolution; i++)
            {
                float t = i / (float)(curveResolution - 1);
                Vector3 point = CalculateQuadraticBezierPoint(startPosition, midPoint, targetPosition, t);
                
                // Add subtle variation to middle points only
                if (i > 0 && i < curveResolution - 1)
                {
                    float noiseOffset = Mathf.PerlinNoise(t * 2f + Time.time * 0.5f, 0f) * 0.02f;
                    point += Vector3.right * noiseOffset;
                }
                
                lineRenderer.SetPosition(i, point);
            }

            // Smooth object movement
            levitatedObject.transform.position = Vector3.Lerp(
                levitatedObject.transform.position, 
                startPosition + direction * initialDistance, 
                Time.deltaTime * 10f
            );
        }
    }

    private Vector3 CalculateQuadraticBezierPoint(Vector3 start, Vector3 control, Vector3 end, float t)
    {
        // Calculate point along a quadratic Bezier curve
        float oneMinusT = 1f - t;
        return
            oneMinusT * oneMinusT * start +
            2f * oneMinusT * t * control +
            t * t * end;
    }   
    
    private Vector3 CalculateReleaseVelocity()
    {
        Vector3 velocitySum = Vector3.zero;
        int validSamples = 0;

        // Calculate velocity from position history
        for (int i = 1; i < positionCount; i++)
        {
            int currentIndex = (currentPositionIndex - i + positionCount) % positionCount;
            int previousIndex = (currentPositionIndex - i - 1 + positionCount) % positionCount;
            
            Vector3 frameDelta = previousWandPositions[currentIndex] - previousWandPositions[previousIndex];
            if (frameDelta.magnitude < 1f) // Ignore large jumps that might be errors
            {
                velocitySum += frameDelta;
                validSamples++;
            }
        }

        // Calculate and limit the release velocity
        Vector3 averageVelocity = validSamples > 0 ? velocitySum / validSamples : Vector3.zero;
        Vector3 releaseVelocity = averageVelocity * releaseForceMultiplier / Time.deltaTime;

        // Cap the velocity magnitude to prevent excessive throwing force
        if (releaseVelocity.magnitude > maxThrowVelocity)
        {
            releaseVelocity = releaseVelocity.normalized * maxThrowVelocity;
        }

        return releaseVelocity;
    }

    private void ReleaseLevitatedObject(Vector3 releaseVelocity)
    {
        if (levitatedRigidbody != null)
        {
            levitatedRigidbody.useGravity = true;
            levitatedRigidbody.isKinematic = false;
            
            // Apply the release velocity
            levitatedRigidbody.velocity = releaseVelocity;

            // Clear references
            levitatedObject = null;
            levitatedRigidbody = null;
        }

        CleanupActiveSpell();
    }

}