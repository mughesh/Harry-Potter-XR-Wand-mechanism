using UnityEngine;
using System;

public class SpellProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float maxDistance = 100f;
    public float curveHeight = 0.5f; // Reduced from 1f to 0.5f
    public float curveVariation = 0.2f; // New variable to control randomness

    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 controlPoint;
    private float startTime;
    private float journeyLength;

    public event Action<Collision> OnSpellHit;

    public void Initialize(Transform wandTip)
    {
        startPoint = wandTip.position;
        
        RaycastHit hit;
        if (Physics.Raycast(wandTip.position, wandTip.forward, out hit, maxDistance))
        {
            endPoint = hit.point;
            //Debug.Log($"Raycast hit: {hit.collider.name} at distance {hit.distance}");
            //Debug.DrawLine(wandTip.position, hit.point, Color.red, 5f);
        }
        else
        {
            endPoint = wandTip.position + wandTip.forward * maxDistance;
            //Debug.Log($"Raycast did not hit anything, using max distance: {maxDistance}");
           // Debug.DrawLine(wandTip.position, endPoint, Color.blue, 5f);
        }

        Vector3 midPoint = (startPoint + endPoint) / 2f;
        Vector3 upDirection = Vector3.Cross(wandTip.right, (endPoint - startPoint).normalized).normalized;
        controlPoint = midPoint + upDirection * curveHeight;
        
        // Add some controlled randomness to the control point
        controlPoint += UnityEngine.Random.insideUnitSphere * curveVariation;

        startTime = Time.time;

        // Calculate the length of the journey
        journeyLength = EstimateJourneyLength();

        //Debug.Log($"Projectile initialized. Start: {startPoint}, End: {endPoint}, Control: {controlPoint}, Journey Length: {journeyLength}");
    }

    void Update()
    {
        float distanceCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distanceCovered / journeyLength;

        if (fractionOfJourney > 1f)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = CalculatePosition(fractionOfJourney);

        // Orient the projectile along its path
        if (fractionOfJourney < 0.99f)
        {
            Vector3 nextPosition = CalculatePosition(fractionOfJourney + 0.01f);
            transform.LookAt(nextPosition);
        }
    }

    private Vector3 CalculatePosition(float t)
    {
        Vector3 m1 = Vector3.Lerp(startPoint, controlPoint, t);
        Vector3 m2 = Vector3.Lerp(controlPoint, endPoint, t);
        return Vector3.Lerp(m1, m2, t);
    }

    private float EstimateJourneyLength()
    {
        float length = 0;
        int steps = 10;
        Vector3 previousPoint = startPoint;
        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector3 point = CalculatePosition(t);
            length += Vector3.Distance(previousPoint, point);
            previousPoint = point;
        }
        return length;
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnSpellHit?.Invoke(collision);
        Destroy(gameObject);
    }
}