using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InventorySlot : MonoBehaviour
{
    public InventoryItemData currentItem;
    private XRGrabInteractable grabInteractable;
    private MeshRenderer slotRenderer;
    private InventorySystem inventorySystem;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        }
        
        slotRenderer = GetComponent<MeshRenderer>();
        inventorySystem = FindObjectOfType<InventorySystem>();

        grabInteractable.selectEntered.AddListener(OnSlotGrabbed);
        
        // Initially hide the slot
        SetSlotVisibility(false);
    }

    public void SetItem(InventoryItemData item)
    {
        currentItem = item;
        if (item != null)
        {
            slotRenderer.material.mainTexture = item.inventorySprite.texture;
            SetSlotVisibility(true);
        }
        else
        {
            SetSlotVisibility(false);
        }
    }

    private void SetSlotVisibility(bool isVisible)
    {
        slotRenderer.enabled = isVisible;
        grabInteractable.enabled = isVisible;
    }

    private void OnSlotGrabbed(SelectEnterEventArgs args)
    {
        if (currentItem != null)
        {
            inventorySystem.SpawnItem(currentItem, args.interactorObject.transform.position);
            SetItem(null);
        }
    }
}