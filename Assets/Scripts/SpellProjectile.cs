using UnityEngine;
using System;

public class SpellProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float maxDistance = 100f;
    public float curveHeight = 1f;
    public float duration = 1f;

    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 controlPoint;
    private float startTime;

    public event Action<Collision> OnSpellHit;

    public void Initialize(Transform wandTip)
    {
        startPoint = wandTip.position;
        
        RaycastHit hit;
        if (Physics.Raycast(wandTip.position, wandTip.forward, out hit, maxDistance))
        {
            endPoint = hit.point;
            print(endPoint);
        }
        else
        {
            endPoint = wandTip.position + wandTip.forward * maxDistance;
        }

        Vector3 midPoint = (startPoint + endPoint) / 2f;
        controlPoint = midPoint + (UnityEngine.Random.insideUnitSphere * curveHeight);

        startTime = Time.time;
    }

    void Update()
    {
        float t = (Time.time - startTime) / duration;
        if (t > 1f)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 m1 = Vector3.Lerp(startPoint, controlPoint, t);
        Vector3 m2 = Vector3.Lerp(controlPoint, endPoint, t);
        transform.position = Vector3.Lerp(m1, m2, t);

        // Orient the projectile along its path
        if (t < 1f)
        {
            Vector3 nextPosition = CalculatePosition(t + 0.01f);
            transform.LookAt(nextPosition);
        }
    }

    private Vector3 CalculatePosition(float t)
    {
        Vector3 m1 = Vector3.Lerp(startPoint, controlPoint, t);
        Vector3 m2 = Vector3.Lerp(controlPoint, endPoint, t);
        return Vector3.Lerp(m1, m2, t);
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnSpellHit?.Invoke(collision);
        Destroy(gameObject);
    }
}