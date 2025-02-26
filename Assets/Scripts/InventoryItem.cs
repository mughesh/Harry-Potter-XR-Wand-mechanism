using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class InventoryItem : MonoBehaviour
{
    public InventoryItemData itemData;
    private Vector3 originalScale;
    private bool isInBookCollider = false;
    private bool isGrabbed = false;
    private Rigidbody rb;
    private InventorySystem inventorySystem;
    private Coroutine scaleCoroutine;
    private Vector3 targetScale;
    private XRGrabInteractable grabInteractable;
    private BookController bookController;
    
    [SerializeField] private float scaleAnimationDuration = 0.3f;
    [SerializeField] private float inventoryScalePercentage = 10f;
    
    public Vector3 OriginalScale => originalScale;
    public bool IsInSlot { get; private set; }
    public Transform CurrentSlot { get; private set; }

    private void Start()
    {
        inventorySystem = FindObjectOfType<InventorySystem>();
        if (inventorySystem == null)
        {
            Debug.LogError("InventorySystem not found!");
            return;
        }

        bookController = FindObjectOfType<BookController>();
        if (bookController == null)
        {
            Debug.LogWarning("BookController not found! Book grab state check will be disabled.");
        }

        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;
        
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            SetupGrabInteractable();
        }
    }

    private void SetupGrabInteractable()
    {
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
        
        grabInteractable.throwOnDetach = false;
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = true;
        grabInteractable.retainTransformParent = true;
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        transform.SetParent(null);
        
        if (IsInSlot)
        {   
            Debug.Log("Grabbed item in slot: " + name);
            StartScaleAnimation(originalScale);
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;

        if (isInBookCollider && IsBookActive())
        {
            if (!IsInSlot)
            {
                inventorySystem.AddItemViaHand(this);
                SetPhysicsState(false);
            }
        }
        else
        {
            if (IsInSlot)
            {
                // If released outside while in slot, retrieve it
                inventorySystem.RetrieveItemViaHand(this);
                transform.localScale = originalScale;
                SetPhysicsState(true);
            }
            else
            {
                transform.localScale = originalScale;
                SetPhysicsState(true);
            }
        }
    }

    // Check if the book is active (being held/grabbed)
    private bool IsBookActive()
    {
        if (bookController == null) return true; // If no book controller found, default to true
        
        // Check if the book is currently being grabbed
        var bookGrabInteractable = bookController.GetComponent<XRGrabInteractable>();
        if (bookGrabInteractable != null)
        {
            return bookGrabInteractable.isSelected;
        }
        
        return true; // Default to true if we can't determine
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BookCollider") && isGrabbed && !IsInSlot)
        {
            // Only scale if the book is being held
            if (IsBookActive())
            {
                isInBookCollider = true;
                Vector3 inventoryScale = originalScale * (inventoryScalePercentage / 100f);
                StartScaleAnimation(inventoryScale);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("BookCollider") && isGrabbed && !IsInSlot)
        {
            // Only scale if the book is being held
            if (IsBookActive())
            {
                isInBookCollider = true;
                Vector3 inventoryScale = originalScale * (inventoryScalePercentage / 100f);
                // No need to start scale animation here, just maintain the scale
                transform.localScale = inventoryScale;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("BookCollider") && !IsInSlot)
        {
            isInBookCollider = false;
            if (isGrabbed)
            {
                StartScaleAnimation(originalScale);
                transform.localScale = originalScale;
            }
        }
    }

    private void StartScaleAnimation(Vector3 newTargetScale)
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        targetScale = newTargetScale;
        scaleCoroutine = StartCoroutine(AnimateScale());
    }

    private IEnumerator AnimateScale()
    {
        Vector3 startScale = transform.localScale;
        float elapsedTime = 0f;
        
        while (elapsedTime < scaleAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / scaleAnimationDuration;
            float smoothStep = Mathf.SmoothStep(0, 1, normalizedTime);
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, smoothStep);
            yield return null;
        }

        transform.localScale = targetScale;
        scaleCoroutine = null;
    }

    private void SetPhysicsState(bool usePhysics)
    {
        if (rb != null)
        {
            rb.isKinematic = !usePhysics;
            rb.useGravity = usePhysics;
        }
        
        // Also handle any colliders on this object and child objects
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            // Enable/disable collision detection based on physics state
            // You might want to keep triggers enabled for interaction
            if (!collider.isTrigger)
            {
                collider.enabled = usePhysics || IsInSlot;
            }
        }
    }

    public void SetInventoryState(bool inInventory, Transform slot)
    {
        IsInSlot = inInventory;
        Debug.Log("IsInSlot: " + IsInSlot);
        CurrentSlot = slot;
        Debug.Log("CurrentSlot: " + CurrentSlot);

        if (inInventory)
        {
            transform.localScale = originalScale * (inventoryScalePercentage / 100f);
            SetPhysicsState(false);
        }
        else
        {
            StartScaleAnimation(originalScale);
            SetPhysicsState(true);
        }
    }
}