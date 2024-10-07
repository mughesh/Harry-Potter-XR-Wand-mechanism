using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class InventoryItem : MonoBehaviour
{
    public InventoryItemData itemData;
    private XRGrabInteractable grabInteractable;
    private Vector3 originalScale;
    private Transform originalParent;

    [SerializeField] private float scaleDuration = 0.5f;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float finalScale = 0.5f;

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        originalScale = transform.localScale;
        originalParent = transform.parent;
    }

    public void AddToInventory(Transform slot)
    {
        StartCoroutine(AddToInventoryCoroutine(slot));
    }

    private IEnumerator AddToInventoryCoroutine(Transform slot)
    {
        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.localScale;
        
        float elapsedTime = 0f;
        while (elapsedTime < scaleDuration)
        {
            float t = elapsedTime / scaleDuration;
            transform.localScale = Vector3.Lerp(startScale, originalScale * finalScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            transform.position = Vector3.Lerp(startPosition, slot.position, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.SetParent(slot);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        grabInteractable.enabled = false;
    }

    public void RetrieveFromInventory(Transform newParent)
    {
        transform.SetParent(newParent);
        transform.localScale = originalScale;
        grabInteractable.enabled = true;
    }
}