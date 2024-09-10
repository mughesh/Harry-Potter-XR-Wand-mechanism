using UnityEngine;
using System.Collections;

public abstract class BaseSpell : MonoBehaviour
{
    public abstract string SpellName { get; }
    public abstract string Description { get; }

    public GameObject equipVFXPrefab;
    public GameObject castVFXPrefab;
    public GameObject hitVFXPrefab;

    protected GameObject activeEquipVFX;
    protected GameObject activeCastVFX;

    public float speed = 10f;
    public float maxDistance = 100f;
    public float curveHeight = 0.5f;
    public float curveVariation = 0.2f;

    public virtual void Equip()
    {
        if (equipVFXPrefab != null)
        {
            activeEquipVFX = Instantiate(equipVFXPrefab, transform.position, transform.rotation, transform);
            Debug.Log("BASE SPELL Equipped" + SpellName);
        }
        else
        {
            Debug.LogWarning($"{SpellName}: equipVFXPrefab is null");
        }
    }

    public virtual void Cast(Transform wandTip)
    {
        if (castVFXPrefab != null)
        {
            activeCastVFX = Instantiate(castVFXPrefab, wandTip.position, wandTip.rotation);
            StartCoroutine(MoveAlongCurve(wandTip));
            Debug.Log("BASE SPELL Casted" + SpellName);
        }
    }

    public virtual void Hold() { }

    public virtual void Release()
    {
        if (activeEquipVFX != null)
        {
            Destroy(activeEquipVFX);
        }
    }

    protected virtual void HandleSpellHit(RaycastHit hit)
    {
        if (hitVFXPrefab != null)
        {
            Instantiate(hitVFXPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }

    private IEnumerator MoveAlongCurve(Transform wandTip)
    {   
        Debug.Log("BASE SPELL Moving along curve");
        Vector3 startPoint = wandTip.position;
        Vector3 endPoint;
        RaycastHit hit;
        if (Physics.Raycast(wandTip.position, wandTip.forward, out hit, maxDistance))
        {
            endPoint = hit.point;
            Debug.Log($"{SpellName}: Raycast hit at {hit.point}");
        }
        else
        {
            endPoint = wandTip.position + wandTip.forward * maxDistance;
        }

        Vector3 midPoint = (startPoint + endPoint) / 2f;
        Vector3 upDirection = Vector3.Cross(wandTip.right, (endPoint - startPoint).normalized).normalized;
        Vector3 controlPoint = midPoint + upDirection * curveHeight + Random.insideUnitSphere * curveVariation;

        float journeyLength = Vector3.Distance(startPoint, endPoint);
        float startTime = Time.time;

        while (activeCastVFX != null)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength;

            if (fractionOfJourney >= 1f)
            {
                if (hit.collider != null)
                {
                    HandleSpellHit(hit);
                }
                Destroy(activeCastVFX);
                break;
            }

            Vector3 m1 = Vector3.Lerp(startPoint, controlPoint, fractionOfJourney);
            Vector3 m2 = Vector3.Lerp(controlPoint, endPoint, fractionOfJourney);
            activeCastVFX.transform.position = Vector3.Lerp(m1, m2, fractionOfJourney);

            if (fractionOfJourney < 0.99f)
            {
                Vector3 nextPosition = Vector3.Lerp(Vector3.Lerp(m1, m2, fractionOfJourney + 0.01f), 
                                                    Vector3.Lerp(m2, endPoint, fractionOfJourney + 0.01f), 
                                                    fractionOfJourney + 0.01f);
                activeCastVFX.transform.LookAt(nextPosition);
            }

            yield return null;
        }
    }
}