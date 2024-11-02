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
        
        if (IsInSlot)
        {
            inventorySystem.RetrieveItemViaHand(this);
            StartScaleAnimation(originalScale);
            SetPhysicsState(false);
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;

        if (isInBookCollider)
        {
            if (!IsInSlot)
            {
                inventorySystem.AddItemViaHand(this);
                SetPhysicsState(true);
            }
        }
        else
        {
            if (IsInSlot)
            {
                // If released outside while in slot, retrieve it
                inventorySystem.RetrieveItemViaHand(this);
                SetPhysicsState(false);
            }
            else
            {
                // Just enable physics if not in slot
                SetPhysicsState(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BookCollider") && isGrabbed && !IsInSlot)
        {
            isInBookCollider = true;
            Vector3 inventoryScale = originalScale * (inventoryScalePercentage / 100f);
            StartScaleAnimation(inventoryScale);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("BookCollider") && isGrabbed && !IsInSlot)
        {
            isInBookCollider = true;
            Vector3 inventoryScale = originalScale * (inventoryScalePercentage / 100f);
            // No need to start scale animation here, just maintain the scale
            transform.localScale = inventoryScale;
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

    private void SetPhysicsState(bool inInventory)
    {
        if (rb != null)
        {
            rb.isKinematic = inInventory;
            rb.useGravity = !inInventory;
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
            SetPhysicsState(true);
        }
        else
        {
            StartScaleAnimation(originalScale);
            SetPhysicsState(false);
        }
    }

    // Method for wand-based interaction
    // public void OnWandSelect()
    // {
    //     if (IsInSlot)
    //     {
    //         inventorySystem.RetrieveItemViaWand(this, null);
    //     }
    //     else if (isInBookCollider)
    //     {
    //         inventorySystem.AddItemViaWand(this);
    //     }
    // }
}