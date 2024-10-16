using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InventoryItem : MonoBehaviour
{
    public InventoryItemData itemData;
    private Vector3 originalScale;
    private bool isInBookCollider = false;
    private bool isGrabbed = false;
    private Rigidbody rb;
<<<<<<< Updated upstream
    private Coroutine scaleCoroutine;
=======
    private InventorySystem inventorySystem;
>>>>>>> Stashed changes
    
    public Vector3 OriginalScale => originalScale;
    public bool IsInSlot { get; private set; }

    private void Start()
    {
<<<<<<< Updated upstream
        grabInteractable = GetComponent<XRGrabInteractable>();
=======
        inventorySystem = FindObjectOfType<InventorySystem>();
        if (inventorySystem == null)
        {
            Debug.LogError("InventorySystem not found!");
            return;
        }

>>>>>>> Stashed changes
        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;
        
        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        if (IsInSlot)
        {
            inventorySystem.RetrieveItemViaHand(this);
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        if (isInBookCollider)
        {
            inventorySystem.AddItemViaHand(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isGrabbed && other.CompareTag("BookCollider"))
        {
            isInBookCollider = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("BookCollider"))
        {
            isInBookCollider = false;
        }
    }

<<<<<<< Updated upstream
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
        IsInSlot = true;
    }


    public void RetrieveFromInventory(Vector3 targetPosition)
    {
        StartCoroutine(RetrieveFromInventoryCoroutine(targetPosition));
    }

    public void RetrieveFromInventoryWithWand(Transform playerTransform)
    {
        if (IsInSlot)
        {
            Vector3 retrievalPosition = playerTransform.position + playerTransform.forward * 1.5f + Vector3.up * 1.2f;
            StartCoroutine(RetrieveFromInventoryCoroutine(retrievalPosition));
        }
    }

    private IEnumerator RetrieveFromInventoryCoroutine(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.localScale;
        
        EnablePhysics();
        transform.SetParent(null);
        IsInSlot = false;

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
=======
    public void EnablePhysics()
>>>>>>> Stashed changes
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    public void DisablePhysics()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void SetInventoryState(bool inInventory, Transform slot)
    {
        IsInSlot = inInventory;
        CurrentSlot = slot;
    }
}