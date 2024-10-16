using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    [SerializeField] private Transform[] inventorySlots;
    [SerializeField] private float scaleDuration = 0.5f;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float inventoryScalePercentage = 10f;
    
    public Dictionary<InventoryItem, Transform> itemSlotMap = new Dictionary<InventoryItem, Transform>();
    private int currentSlotIndex = 0;

    // Core inventory management
    public Transform GetAvailableSlot()
    {
        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            Debug.LogError("No inventory slots configured!");
            return null;
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            int index = (currentSlotIndex + i) % inventorySlots.Length;
            Transform slot = inventorySlots[index];
            
            if (slot == null) continue;

            XRSocketInteractor socket = slot.GetComponent<XRSocketInteractor>();
            if (socket != null && socket.interactablesSelected.Count == 0)
            {
                currentSlotIndex = (index + 1) % inventorySlots.Length;
                return slot;
            }
        }
        return null;
    }

    // Add item methods
    public void AddItemViaHand(InventoryItem item)
    {
        Transform slot = GetAvailableSlot();
        if (slot != null)
        {
            StartCoroutine(AddItemToSlotCoroutine(item, slot));
        }
    }

    public void AddItemViaWand(InventoryItem item)
    {
        Transform slot = GetAvailableSlot();
        if (slot != null)
        {
            StartCoroutine(AddItemToSlotCoroutine(item, slot));
        }
    }

    private IEnumerator AddItemToSlotCoroutine(InventoryItem item, Transform slot)
    {
        Vector3 startPosition = item.transform.position;
        Vector3 startScale = item.transform.localScale;
        Vector3 targetScale = item.OriginalScale * (inventoryScalePercentage / 100f);

        // Animation
        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            item.transform.position = Vector3.Lerp(startPosition, slot.position, t);
            item.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Finalize and set parent
        item.transform.SetParent(slot);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = targetScale;
        item.DisablePhysics();
        itemSlotMap[item] = slot;
        item.SetInventoryState(true, slot);
    }

    // Retrieve item methods
    public void RetrieveItemViaHand(InventoryItem item)
    {
        if (itemSlotMap.ContainsKey(item))
        {
            item.transform.SetParent(null);
            item.EnablePhysics();
            itemSlotMap.Remove(item);
            item.SetInventoryState(false, null);
            StartCoroutine(ScaleToOriginal(item));
        }
    }

    public void RetrieveItemViaWand(InventoryItem item, Transform wandTransform)
    {
        if (itemSlotMap.ContainsKey(item))
        {
            item.transform.SetParent(null); 
            Vector3 retrievalPosition = wandTransform.position + wandTransform.forward * 1.5f + Vector3.up * 1.2f;
            StartCoroutine(RetrieveItemCoroutine(item, retrievalPosition));
            itemSlotMap.Remove(item);
            item.SetInventoryState(false, null);
        }
    }

    private IEnumerator RetrieveItemCoroutine(InventoryItem item, Vector3 targetPosition)
    {
        Vector3 startPosition = item.transform.position;
        Vector3 startScale = item.transform.localScale;
        
        item.EnablePhysics();

        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            item.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            item.transform.localScale = Vector3.Lerp(startScale, item.OriginalScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ScaleToOriginal(InventoryItem item)
    {
        Vector3 startScale = item.transform.localScale;
        
        float elapsedTime = 0f;
        while (elapsedTime < scaleDuration)
        {
            float t = elapsedTime / scaleDuration;
            item.transform.localScale = Vector3.Lerp(startScale, item.OriginalScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void UpdateInventoryParenting()
    {
        foreach (var kvp in itemSlotMap)
        {
            InventoryItem item = kvp.Key;
            Transform slot = kvp.Value;

            if (item != null && slot != null)
            {
                item.transform.SetParent(slot);
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;
            }
        }
    }
    public void UpdateItemsVisibility(bool isInventoryOpen)
    {
        foreach (var kvp in itemSlotMap)
        {
            if (kvp.Key != null)
            {
                kvp.Key.gameObject.SetActive(isInventoryOpen);
            }
        }
    }
}