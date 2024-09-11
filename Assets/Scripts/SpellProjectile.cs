using UnityEngine;
using System;

public class SpellProjectile : MonoBehaviour
{
    public float curveHeight = 0.5f;
    public float curveVariation = 0.2f;

    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 controlPoint;
    private float startTime;
    private float journeyLength;
    private float speed;

    public event Action<RaycastHit> OnProjectileHit;

    public void Initialize(Transform spawnPoint, float projectileSpeed, float maxDistance)
    {
        speed = projectileSpeed;
        startPoint = spawnPoint.position;
        
        RaycastHit hit;
        if (Physics.Raycast(spawnPoint.position, spawnPoint.forward, out hit, maxDistance))
        {
            endPoint = hit.point;
        }
        else
        {
            endPoint = spawnPoint.position + spawnPoint.forward * maxDistance;
        }

        Vector3 midPoint = (startPoint + endPoint) / 2f;
        Vector3 upDirection = Vector3.Cross(spawnPoint.right, (endPoint - startPoint).normalized).normalized;
        controlPoint = midPoint + upDirection * curveHeight;
        controlPoint += UnityEngine.Random.insideUnitSphere * curveVariation;

        startTime = Time.time;
        journeyLength = EstimateJourneyLength();
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

        Vector3 newPosition = CalculatePosition(fractionOfJourney);
        
        if (float.IsNaN(newPosition.x) || float.IsNaN(newPosition.y) || float.IsNaN(newPosition.z))
        {
            Debug.LogWarning("Invalid position calculated. Destroying projectile.");
            Destroy(gameObject);
            return;
        }

        transform.position = newPosition;

        if (fractionOfJourney < 0.99f)
        {
            Vector3 nextPosition = CalculatePosition(fractionOfJourney + 0.01f);
            transform.LookAt(nextPosition);
        }

        CheckCollision();
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

    private void CheckCollision()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, speed * Time.deltaTime))
        {
            OnProjectileHit?.Invoke(hit);
            Destroy(gameObject);
        }
    }
}