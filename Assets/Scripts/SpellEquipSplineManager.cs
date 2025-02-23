using UnityEngine;
using UnityEngine.Splines;
using System.Collections;
using System.Collections.Generic;

public class SpellEquipSplineManager : MonoBehaviour
{
    [Header("Spline References")]
    private List<GameObject> activeSplineVFX = new List<GameObject>();
    private GameObject currentTipVFX;
    private Coroutine[] activeAnimationCoroutines;

    [SerializeField] private SplineContainer[] helixSplines; // Reference to the 3 helix splines
    [SerializeField] private Transform wandTip;
    
    [Header("Animation Settings")]
    [SerializeField] private float splineDuration = 0.5f;
    [SerializeField] private float delayBetweenSplines = 0.1f;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private WandController wandController;
    private SpellSystem spellSystem;

    private void Start()
    {
        wandController = GetComponent<WandController>();
        spellSystem = FindObjectOfType<SpellSystem>();

        // Verify components
        if (helixSplines == null || helixSplines.Length != 3)
        {
            Debug.LogError("Please assign exactly 3 helix splines in the inspector!");
        }
    }

    public void CleanupAllEffects()
    {
        // Stop all active coroutines
        if (activeAnimationCoroutines != null)
        {
            foreach (var coroutine in activeAnimationCoroutines)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
            }
        }

        // Cleanup spline VFX
        foreach (var vfx in activeSplineVFX)
        {
            if (vfx != null)
                Destroy(vfx);
        }
        activeSplineVFX.Clear();

        // Cleanup tip VFX
        if (currentTipVFX != null)
        {
            Destroy(currentTipVFX);
            currentTipVFX = null;
        }
    }

    public IEnumerator AnimateSpellEquip(SpellData spell)
    {
        // Cleanup previous effects first
        CleanupAllEffects();

        if (spell.equipVFXPrefab == null) yield break;

        // Create VFX instances for each spline
        activeAnimationCoroutines = new Coroutine[helixSplines.Length];
        
        for (int i = 0; i < helixSplines.Length; i++)
        {
            // Create VFX at spline start
            Vector3 startPos = helixSplines[i].EvaluatePosition(0f);
            GameObject vfx = Instantiate(spell.equipVFXPrefab, startPos, Quaternion.identity);
            activeSplineVFX.Add(vfx);
            
            // Start animation with delay between each spline
            activeAnimationCoroutines[i] = StartCoroutine(AnimateAlongSpline(vfx, helixSplines[i], i * delayBetweenSplines));
        }

        // Wait for all animations to complete
        yield return new WaitForSeconds(splineDuration + (delayBetweenSplines * 2));

        // Clean up spline VFX
        foreach (var vfx in activeSplineVFX)
        {
            if (vfx != null)
                Destroy(vfx);
        }
        activeSplineVFX.Clear();

        // Now create the final equip effect at wand tip as per original system
        if (spell.equipVFXPrefab != null && wandTip != null)
        {
            if (currentTipVFX != null)
                Destroy(currentTipVFX);
                
            currentTipVFX = Instantiate(spell.equipVFXPrefab, wandTip.position, wandTip.rotation);
            currentTipVFX.transform.SetParent(wandTip, true);
            currentTipVFX.transform.localPosition = Vector3.zero;
            currentTipVFX.transform.localRotation = Quaternion.identity;
        }
    }

    private IEnumerator AnimateAlongSpline(GameObject vfx, SplineContainer spline, float startDelay)
    {
        yield return new WaitForSeconds(startDelay);

        float elapsedTime = 0f;
        
        while (elapsedTime < splineDuration)
        {
            float normalizedTime = elapsedTime / splineDuration;
            float curvedTime = speedCurve.Evaluate(normalizedTime);
            
            // Get position and rotation from spline
            Vector3 position = spline.EvaluatePosition(curvedTime);
            Vector3 tangent = spline.EvaluateTangent(curvedTime);
            Vector3 up = spline.EvaluateUpVector(curvedTime);
            
            // Update VFX transform
            vfx.transform.position = position;
            if (tangent != Vector3.zero)
            {
                vfx.transform.rotation = Quaternion.LookRotation(tangent, up);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final position is at end of spline
        vfx.transform.position = spline.EvaluatePosition(1f);
    }
}