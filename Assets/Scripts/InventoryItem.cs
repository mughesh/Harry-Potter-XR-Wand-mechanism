using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class InventoryItem : MonoBehaviour
{
    public InventoryItemData itemData;
    private XRGrabInteractable grabInteractable;
    private Vector3 originalScale;
    private Transform originalParent;
    private XRSocketInteractor currentSocket;
    private bool isInBookCollider = false;
    private bool isGrabbed = false;
    private Rigidbody rb;
    private Coroutine scaleCoroutine;
    
    [SerializeField] private float scaleDuration = 0.5f;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float inventoryScalePercentage = 10f;

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;
        originalParent = transform.parent;

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);

        EnablePhysics();
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        if (isInBookCollider)
        {
            BookController bookController = FindObjectOfType<BookController>();
            if (bookController != null)
            {
                Transform availableSlot = bookController.GetAvailableSlot();
                if (availableSlot != null)
                {
                    AddToInventory(availableSlot);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isGrabbed == true && other.CompareTag("BookCollider"))
        {
            isInBookCollider = true;
            StartScaling(inventoryScalePercentage / 100f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isGrabbed == true && other.CompareTag("BookCollider"))
        {
            // Ensure the object maintains its small scale while in the collider
            transform.localScale = originalScale * (inventoryScalePercentage / 100f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isGrabbed == true && other.CompareTag("BookCollider"))
        {
            isInBookCollider = false;
            StartScaling(1f);
        }
    }

    private void StartScaling(float targetScalePercentage)
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(ScaleTo(targetScalePercentage));
    }

    public void AddToInventory(Transform slot)
    {
        StartCoroutine(AddToInventoryCoroutine(slot));
    }

    private IEnumerator AddToInventoryCoroutine(Transform slot)
    {
        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = originalScale * (inventoryScalePercentage / 100f);
        
        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            transform.position = Vector3.Lerp(startPosition, slot.position, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.SetParent(slot);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = targetScale;

        currentSocket = slot.GetComponent<XRSocketInteractor>();
        DisablePhysics();
    }

    public void RetrieveFromInventory(Vector3 targetPosition)
    {
        StartCoroutine(RetrieveFromInventoryCoroutine(targetPosition));
    }

    private IEnumerator RetrieveFromInventoryCoroutine(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.localScale;
        
        EnablePhysics();
        transform.SetParent(null);

        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.localScale = Vector3.Lerp(startScale, originalScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private IEnumerator ScaleTo(float targetScalePercentage)
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = originalScale * targetScalePercentage;
        
        float elapsedTime = 0f;
        while (elapsedTime < scaleDuration)
        {
            float t = elapsedTime / scaleDuration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void EnablePhysics()
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    private void DisablePhysics()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }
}