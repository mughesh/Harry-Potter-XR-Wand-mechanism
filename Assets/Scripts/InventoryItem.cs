using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InventoryItem : MonoBehaviour
{
    public InventoryItemData itemData;
    private Vector3 originalScale;
    private bool isInBookCollider = false;
    private bool isGrabbed = false;
    private Rigidbody rb;
    private InventorySystem inventorySystem;
    
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

    public void EnablePhysics()
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