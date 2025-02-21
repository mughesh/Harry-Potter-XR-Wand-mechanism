using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;

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

    [Header("Projectile Path Controls")]
    [SerializeField] private float basePathHeight = 2f; // Base height of the curve
    [SerializeField][Range(0.1f, 1f)] private float distanceHeightRatio = 0.3f; // How much distance affects height
    [SerializeField][Range(0.1f, 1f)] private float minCurveHeight = 0.2f; // Minimum curve height for close targets
    [SerializeField] private float projectileSpeed = 10f; // Configurable projectile speed
    [SerializeField] private float pathRandomness = 0.5f; // Controls how random the path can be
    [SerializeField] private Vector2 sideOffsetRange = new Vector2(-2f, 2f); // Range for side-to-side movement
    [SerializeField] private bool useUpwardPath = false; // Toggle for upward/sideways paths

    [Header("Effect Controls")]
    [SerializeField] private float hitEffectDuration = 2f;

    private GameObject currentEquipEffect;
    private Transform wandTip;
    private List<(GameObject effect, float endTime)> persistentEffects = new List<(GameObject, float)>();


    [SerializeField] private int curveResolution = 20; // Controls how smooth the curve looks
    [SerializeField] private float curvatureAmount = 0.5f;

    private ParticleSystem activeSpellParticles;
    private bool isFadingOut = false;


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

        // Cache wand tip reference
        WandController wandController = FindObjectOfType<WandController>();
        if (wandController != null)
        {
            wandTip = wandController.wandTip;
        }
    }

    public void SelectSpell(SpellData spell)
    {
        // Clean up previous spell's equip effect
        if (currentEquipEffect != null)
        {
            Destroy(currentEquipEffect);
            currentEquipEffect = null;
        }

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
        if (currentSpell != null && currentSpell.equipVFXPrefab != null && wandTip != null)
        {
            // Create new equip effect parented to wand tip
            currentEquipEffect = Instantiate(currentSpell.equipVFXPrefab, wandTip.position, wandTip.rotation);
            currentEquipEffect.transform.SetParent(wandTip, true);

            // Reset local position/rotation if needed
            currentEquipEffect.transform.localPosition = Vector3.zero + Vector3.forward * 0f;
            currentEquipEffect.transform.localRotation = Quaternion.identity;
        }
    }

    public void ResetSpell()
    {
        if (currentEquipEffect != null)
        {
            Destroy(currentEquipEffect);
            currentEquipEffect = null;
        }

        CleanupActiveSpell();
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

    // Special handling for Lumos to prevent multiple instantiations
    if (spell.spellName == "Lumos" && activeSpellVFX != null)
    {
        // Just update the existing effect if needed
        UpdateActiveSpell(spell, startPosition, direction);
        return;
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
            if (spell.spellName != "Lumos") // Skip update for Lumos
            {
                CastUtilitySpell(spell, startPosition, direction);
            }
            break;
    }
}

private IEnumerator CastSpellCoroutine(SpellData spell, Vector3 startPosition, Vector3 direction)
{
    if (spell.castVFXPrefab != null)
    {
        // Don't recreate Lumos effect if it exists
        if (spell.spellName == "Lumos" && activeSpellVFX != null)
        {
            yield break;
        }

        // Clean up any existing spell VFX except for Lumos
        if (spell.spellName != "Lumos")
        {
            CleanupActiveSpell();
        }

        // Only create new VFX if needed
        if (activeSpellVFX == null)
        {
            activeSpellVFX = Instantiate(spell.castVFXPrefab, startPosition, Quaternion.LookRotation(direction));
            Debug.Log($"Creating new effect for spell: {spell.spellName}");
        }
        
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

        // Only cleanup if it's not Lumos (Lumos handles its own cleanup)
        if (spell.spellName != "Lumos")
        {
            CleanupActiveSpell();
        }
    }
    else
    {
        Debug.LogWarning("No cast VFX prefab assigned for the spell.");
    }
}


    public void CleanupActiveSpell()
    {
        if (activeSpellVFX != null)
        {
            // Start fade out instead of immediate destruction
            StartCoroutine(FadeOutSpell());
        }

        if (activeLineRenderer != null)
        {
            Destroy(activeLineRenderer);
            activeLineRenderer = null;
        }

        // Don't immediately destroy hit effects, let them fade naturally
        isSpellActive = false;
    }

    public void ForceCleanupAllEffects()
    {
        // Clean up all persistent effects immediately
        foreach (var (effect, _) in persistentEffects)
        {
            if (effect != null)
            {
                Destroy(effect);
            }
        }
        persistentEffects.Clear();

        // Clean up active spell effects
        if (activeSpellVFX != null)
        {
            Destroy(activeSpellVFX);
            activeSpellVFX = null;
        }
        if (activeLineRenderer != null)
        {
            Destroy(activeLineRenderer);
            activeLineRenderer = null;
        }

        activeSpellParticles = null;
        isFadingOut = false;
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

    private Vector3 GenerateControlPoint(Vector3 start, Vector3 end, float distance)
    {
        Vector3 midPoint = (start + end) / 2f;
        Vector3 randomDirection;

        if (useUpwardPath)
        {
            // Traditional upward arc
            randomDirection = Vector3.up;
        }
        else
        {
            // Calculate a random perpendicular direction for side-to-side movement
            Vector3 forward = (end - start).normalized;
            Vector3 right = Vector3.Cross(forward, Vector3.up).normalized;
            float sideOffset = Random.Range(sideOffsetRange.x, sideOffsetRange.y);
            randomDirection = right * sideOffset;
        }

        // Add controlled randomness
        Vector3 randomOffset = Random.insideUnitSphere * pathRandomness;
        randomOffset.y = Mathf.Abs(randomOffset.y); // Keep some upward bias to prevent ground collision

        return midPoint + randomDirection * basePathHeight + randomOffset;
    }

    private IEnumerator CastProjectileSpell(GameObject castVFX, Vector3 startPosition, Vector3 direction)
    {
        try
        {
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

            Vector3 controlPoint = GenerateControlPoint(startPosition, targetPosition, targetDistance);
            List<Vector3> path = GenerateCurvedPath(startPosition, targetPosition, controlPoint, 50);

            int pathIndex = 0;
            bool hitTarget = false;

            while (pathIndex < path.Count && !hitTarget)
            {
                castVFX.transform.position = path[pathIndex];

                if (pathIndex < path.Count - 1)
                {
                    Vector3 nextPosition = path[pathIndex + 1];
                    Vector3 moveDirection = (nextPosition - castVFX.transform.position).normalized;
                    castVFX.transform.forward = moveDirection;
                }

                if (Physics.Raycast(castVFX.transform.position, castVFX.transform.forward, out hit, 0.5f))
                {
                    SpellHitEffect(hit.point, -hit.normal);
                    hitTarget = true;
                }

                pathIndex++;
                yield return new WaitForSeconds(1f / (projectileSpeed * path.Count));
            }

            if (!hitTarget)
            {
                SpellHitEffect(targetPosition, -direction);
            }
        }
        finally
        {
            isProjectileInFlight = false;
        }
    }


    private IEnumerator CastRaySpell(SpellData spell, Vector3 startPosition, Vector3 direction)
    {
        WandController wandController = FindObjectOfType<WandController>();
        Transform wandTip = wandController?.wandTip;

        if (wandTip == null)
        {
            Debug.LogError("Wand tip not found for ray spell!");
            yield break;
        }

        // Create VFX at wand tip
        if (activeSpellVFX == null && spell.castVFXPrefab != null)
        {
            activeSpellVFX = Instantiate(spell.castVFXPrefab, wandTip.position, Quaternion.LookRotation(direction));
            activeSpellParticles = activeSpellVFX.GetComponentInChildren<ParticleSystem>();

            // Don't parent to wandTip, we'll update position manually
            // This gives us more control over the fade-out effect
        }

        if (spell.triggerType == SpellTriggerType.Press)
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(FadeOutSpell());
        }
        else
        {
            // For hold-type, keep updating until spell is deactivated
            while (isSpellActive && !isFadingOut)
            {
                UpdateRaySpell(wandTip.position, wandTip.forward);
                yield return null;
            }
        }
    }

    private void UpdateRaySpell(Vector3 startPosition, Vector3 direction)
    {
        if (activeSpellVFX != null)
        {
            activeSpellVFX.transform.position = startPosition;
            activeSpellVFX.transform.rotation = Quaternion.LookRotation(direction);

            // Check for hit point and create hit effects
            if (Physics.Raycast(startPosition, direction, out RaycastHit hit, maxCastDistance))
            {
                // Create/update hit effect at the raycast hit point
                HandleRayHitEffect(hit.point, -hit.normal);
            }
        }

        if (activeLineRenderer != null)
        {
            LineRenderer lineRenderer = activeLineRenderer.GetComponent<LineRenderer>();
            if (Physics.Raycast(startPosition, direction, out RaycastHit hit, maxCastDistance))
            {
                lineRenderer.SetPosition(0, startPosition);
                lineRenderer.SetPosition(1, hit.point);
            }
            else
            {
                lineRenderer.SetPosition(0, startPosition);
                lineRenderer.SetPosition(1, startPosition + direction * maxCastDistance);
            }
        }
    }

    private GameObject currentHitEffect;
    private Vector3 lastHitPoint;
    private float hitEffectUpdateThreshold = 0.1f; // Minimum distance to update hit effect position

    private void HandleRayHitEffect(Vector3 hitPoint, Vector3 normal)
    {
        if (currentSpell.hitVFXPrefab != null &&
            (currentHitEffect == null || Vector3.Distance(hitPoint, lastHitPoint) > hitEffectUpdateThreshold))
        {
            GameObject newHitEffect = Instantiate(currentSpell.hitVFXPrefab, hitPoint, Quaternion.LookRotation(normal));
            float endTime = Time.time + hitEffectDuration;
            persistentEffects.Add((newHitEffect, endTime));
            lastHitPoint = hitPoint;

            // Start coroutine to remove this specific hit effect
            StartCoroutine(RemoveHitEffectAfterDuration(newHitEffect, hitEffectDuration));
        }
    }

    private IEnumerator RemoveHitEffectAfterDuration(GameObject hitEffect, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (hitEffect != null)
        {
            // Get all particle systems
            ParticleSystem[] particles = hitEffect.GetComponentsInChildren<ParticleSystem>();

            // Stop emission but let existing particles fade
            foreach (var ps in particles)
            {
                var emission = ps.emission;
                emission.enabled = false;
            }

            // Find the longest particle lifetime
            float maxLifetime = 0f;
            foreach (var ps in particles)
            {
                if (ps.main.startLifetime.constant > maxLifetime)
                    maxLifetime = ps.main.startLifetime.constant;
            }

            // Wait for particles to finish
            yield return new WaitForSeconds(maxLifetime);

            // Remove from persistent effects and destroy
            persistentEffects.RemoveAll(x => x.effect == hitEffect);
            Destroy(hitEffect);
        }
    }


    private IEnumerator FadeOutHitEffect(GameObject hitEffect)
    {
        // Optional: Add fade out logic for hit effects
        ParticleSystem[] particles = hitEffect.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            var emission = ps.emission;
            emission.enabled = false;
        }

        yield return new WaitForSeconds(1f); // Adjust time as needed
        Destroy(hitEffect);
    }

private IEnumerator FadeOutSpell()
{
    if (activeSpellVFX == null) yield break;

    isFadingOut = true;
    float fadeOutTime = currentSpell.fadeOutTime; // Shorter fade-out time
    
    // Get all particle systems in the effect
    ParticleSystem[] particleSystems = activeSpellVFX.GetComponentsInChildren<ParticleSystem>();
    
    // Store original rates and lifetimes
    Dictionary<ParticleSystem, float> originalEmissionRates = new Dictionary<ParticleSystem, float>();
    Dictionary<ParticleSystem, ParticleSystem.MinMaxCurve> originalLifetimes = new Dictionary<ParticleSystem, ParticleSystem.MinMaxCurve>();
    
    foreach (var ps in particleSystems)
    {
        // Store original values
        var emission = ps.emission;
        originalEmissionRates[ps] = emission.rateOverTime.constant;
        originalLifetimes[ps] = ps.main.startLifetime;
        
        // Stop new particle emission
        emission.enabled = false;
    }

    float elapsedTime = 0f;
    while (elapsedTime < fadeOutTime)
    {
        elapsedTime += Time.deltaTime;
        float t = elapsedTime / fadeOutTime;

        foreach (var ps in particleSystems)
        {
            // Gradually reduce particle lifetime
            var main = ps.main;
            float originalLifetime = originalLifetimes[ps].constant;
            main.startLifetime = new ParticleSystem.MinMaxCurve(
                Mathf.Lerp(originalLifetime, 0.1f, t)
            );

            // Optionally reduce speed
            main.startSpeedMultiplier = Mathf.Lerp(1f, 0.2f, t);
        }
        
        yield return null;
    }

    // Final cleanup
    yield return new WaitForSeconds(0.1f); // Short wait for last particles
    
    if (activeSpellVFX != null)
    {
        Destroy(activeSpellVFX);
        activeSpellVFX = null;
    }
    
    activeSpellParticles = null;
    isFadingOut = false;
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
    if (isSpellActive && !isFadingOut)
    {
        // Special handling for Lumos
        if (currentSpell != null && currentSpell.spellName == "Lumos")
        {
            StartCoroutine(FadeOutSpell());
        }
        else
        {
            StartCoroutine(FadeOutSpell());
        }
    }
    isSpellActive = false;
}


    // UTILITY SPELLS FUNCTIONS --------------

    // LUMOS
private IEnumerator CastLumos(SpellData spell, Vector3 startPosition, Vector3 direction)
{
    // Only create VFX if it doesn't exist
    if (activeSpellVFX == null)
    {
        Debug.Log("Creating new Lumos effect");
        Transform wandTip = GameObject.FindObjectOfType<WandController>().wandTip;
        if (wandTip == null)
        {
            Debug.LogError("Wand tip not found for Lumos spell!");
            yield break;
        }

        // Create and setup the VFX
        if (spell.castVFXPrefab != null)
        {
            activeSpellVFX = Instantiate(spell.castVFXPrefab, wandTip.position, Quaternion.identity);
            Debug.Log("Lumos effect created");
            activeSpellVFX.transform.SetParent(wandTip, false);
            activeSpellVFX.transform.localPosition = Vector3.zero + Vector3.forward * 0.0f;
            activeSpellVFX.transform.localRotation = Quaternion.identity;
        }
    }

    // Keep the effect active while the spell is active
    while (isSpellActive)
    {
        
        activeSpellVFX.transform.SetParent(wandTip, false);
        activeSpellVFX.transform.localPosition = Vector3.zero + Vector3.forward * 0.0f;
        activeSpellVFX.transform.localRotation = Quaternion.identity;
        yield return null;
    }

    // When spell ends, start fade out
    if (activeSpellVFX != null)
    {
        StartCoroutine(FadeOutSpell());
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
            Debug.Log("Releasing levitated object");
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


    private void OnDisable()
    {
        // Clean up everything when the component is disabled
        CleanupActiveSpell();
        if (currentEquipEffect != null)
        {
            Destroy(currentEquipEffect);
        }
    }
}