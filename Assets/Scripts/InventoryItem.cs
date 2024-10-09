using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class InventoryItem : MonoBehaviour
{
    public InventoryItemData itemData;
    private XRGrabInteractable grabInteractable;
    private Vector3 originalWorldScale;
    private Transform originalParent;
    
    [SerializeField] private float scaleDuration = 0.5f;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float finalInventoryScale = 0.5f;
    
    private bool isInInventory = false;
    private Coroutine currentScalingCoroutine;

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        originalWorldScale = transform.lossyScale;
        originalParent = transform.parent;
        
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    public void AddToInventory(Transform slot)
    {
        if (currentScalingCoroutine != null)
            StopCoroutine(currentScalingCoroutine);
        currentScalingCoroutine = StartCoroutine(AddToInventoryCoroutine(slot));
    }

    private IEnumerator AddToInventoryCoroutine(Transform slot)
    {
        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.lossyScale;
        Vector3 targetScale = originalWorldScale * finalInventoryScale;
        
        yield return ScaleObject(startScale, targetScale);
        yield return MoveObject(startPosition, slot.position);

        transform.SetParent(slot);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one * finalInventoryScale;
        grabInteractable.enabled = false;
        isInInventory = true;
    }

    public void RetrieveFromInventory(Vector3 targetPosition)
    {
        if (currentScalingCoroutine != null)
            StopCoroutine(currentScalingCoroutine);
        currentScalingCoroutine = StartCoroutine(RetrieveFromInventoryCoroutine(targetPosition));
    }

    private IEnumerator RetrieveFromInventoryCoroutine(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.lossyScale;

        transform.SetParent(null);
        grabInteractable.enabled = true;

        yield return ScaleObject(startScale, originalWorldScale);
        yield return MoveObject(startPosition, targetPosition);

        isInInventory = false;
    }

    private IEnumerator ScaleObject(Vector3 startScale, Vector3 endScale)
    {
        float elapsedTime = 0f;
        while (elapsedTime < scaleDuration)
        {
            float t = elapsedTime / scaleDuration;
            SetWorldScale(Vector3.Lerp(startScale, endScale, t));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        SetWorldScale(endScale);
    }

    private IEnumerator MoveObject(Vector3 startPosition, Vector3 endPosition)
    {
        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = endPosition;
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (isInInventory)
        {
            if (currentScalingCoroutine != null)
                StopCoroutine(currentScalingCoroutine);
            currentScalingCoroutine = StartCoroutine(ScaleObject(transform.lossyScale, originalWorldScale));
            isInInventory = false;
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
                if (!isInInventory)
        {
            Collider bookCollider = FindObjectOfType<BookController>().GetComponent<Collider>();
            if (bookCollider.bounds.Contains(transform.position))
            {
                AddToInventory(transform.parent);
            }
        }    }

    private void SetWorldScale(Vector3 worldScale)
    {
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(
            worldScale.x / transform.lossyScale.x,
            worldScale.y / transform.lossyScale.y,
            worldScale.z / transform.lossyScale.z
        );
    }
}