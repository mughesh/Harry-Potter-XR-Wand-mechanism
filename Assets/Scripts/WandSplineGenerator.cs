using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

[RequireComponent(typeof(WandController))]
public class WandSplineGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform wandTip;
    
    [Header("Helix Configuration")]
    [SerializeField] private int pointsPerHelix = 30;
    [SerializeField] private float startRadius = 0.05f;
    [SerializeField] private float endRadius = 0.01f;
    [SerializeField] private float rotationsCount = 2f;
    [SerializeField] private float tangentSmoothing = 0.1f;
    
    [Header("Multi-Helix Settings")]
    [SerializeField] private int numberOfHelixes = 3;
    [SerializeField] private float helixOffset = 120f;

    private List<SplineContainer> splineContainers = new List<SplineContainer>();
    private GameObject splineParent;
    private float helixLength;

    private void Start()
    {
        // Calculate helix length based on wand tip distance
        helixLength = Vector3.Distance(transform.position, wandTip.position);
        InitializeSplines();
    }

    private void InitializeSplines()
    {
        // Create parent object
        splineParent = new GameObject("WandSplines");
        splineParent.transform.SetParent(transform, false);
        
        // Generate helixes
        for (int i = 0; i < numberOfHelixes; i++)
        {
            float angleOffset = i * helixOffset;
            CreateHelixSpline(angleOffset, i);
        }
    }

    private void CreateHelixSpline(float angleOffset, int index)
    {
        GameObject splineObject = new GameObject($"HelixSpline_{index}");
        splineObject.transform.SetParent(splineParent.transform, false);
        SplineContainer splineContainer = splineObject.AddComponent<SplineContainer>();
        
        List<BezierKnot> knots = new List<BezierKnot>();
        Vector3 wandDirection = (wandTip.position - transform.position).normalized;
        
        // Create basis vectors for helix orientation
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.Cross(wandDirection, up).normalized;
        if (right.magnitude < 0.001f)
        {
            right = Vector3.right;
        }
        up = Vector3.Cross(right, wandDirection).normalized;

        for (int i = 0; i < pointsPerHelix; i++)
        {
            float t = i / (float)(pointsPerHelix - 1);
            
            // Calculate radius that decreases along the helix
            float radius = Mathf.Lerp(startRadius, endRadius, t);
            
            // Calculate angle with offset
            float angle = ((t * rotationsCount * 360f) + angleOffset) * Mathf.Deg2Rad;
            
            // Calculate circle position at this point
            float circleX = radius * Mathf.Cos(angle);
            float circleY = radius * Mathf.Sin(angle);
            
            // Calculate final position using basis vectors
            Vector3 circleOffset = right * circleX + up * circleY;
            Vector3 position = transform.position + wandDirection * (t * helixLength) + circleOffset;
            
            // Convert to local space
            position = splineParent.transform.InverseTransformPoint(position);

            // Create knot with auto-smoothing
            BezierKnot knot = new BezierKnot(position);
            knots.Add(knot);
        }
        
        // Create spline with auto tangents
        Spline spline = new Spline(knots, false);
        for (int i = 0; i < spline.Count; i++)
        {
            spline.SetTangentMode(i, TangentMode.AutoSmooth);
        }
        splineContainer.Spline = spline;
        splineContainers.Add(splineContainer);
    }

    private void LateUpdate()
    {
        UpdateSplinePositions();
    }

    private void UpdateSplinePositions()
    {
        if (splineParent != null)
        {
            splineParent.transform.position = transform.position;
            splineParent.transform.rotation = transform.rotation;
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying && wandTip != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(wandTip.position, 0.01f);
            Gizmos.DrawLine(transform.position, wandTip.position);
        }
    }
    #endif

    // Method to show/hide splines
    public void SetSplinesVisible(bool visible)
    {
        if (splineParent != null)
        {
            splineParent.SetActive(visible);
        }
    }
}