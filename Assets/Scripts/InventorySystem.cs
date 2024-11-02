using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using System.Collections;

public class InventorySystem : MonoBehaviour
{
    [SerializeField] private Transform[] inventorySlots;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float retrievalOffset = 0.3f;
    public GameObject retrievePosition;
    private Dictionary<Transform, InventoryItem> slotItemMap = new Dictionary<Transform, InventoryItem>();
    private int currentSlotIndex = 0;

    public Transform GetAvailableSlot()
    {
        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            Debug.LogError("No inventory slots configured!");
            return null;
        }

        // Find first available slot
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (!slotItemMap.ContainsKey(inventorySlots[i]))
            {
                return inventorySlots[i];
            }
        }

        return null;
    }

    // Adding item to inventory

    public void AddItemViaWand(InventoryItem item)
    {   
        Debug.Log("Adding item via wand");
        Transform slot = GetAvailableSlot();
        if (slot != null)
        {
            StartCoroutine(SmoothAddItem(item, slot));
        }
    }

    public void AddItemViaHand(InventoryItem item)
    {
        Transform slot = GetAvailableSlot();
        if (slot != null)
        {
            StartCoroutine(SmoothAddItem(item, slot));
        }
        else
        {
            Debug.LogWarning("No available inventory slots!");
        }
    }

    private IEnumerator SmoothAddItem(InventoryItem item, Transform slot)
    {
        float elapsedTime = 0f;
        Vector3 startPos = item.transform.position;
        Quaternion startRot = item.transform.rotation;
        
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            float smoothT = Mathf.SmoothStep(0, 1, t);
            
            item.transform.position = Vector3.Lerp(startPos, slot.position, smoothT);
            item.transform.rotation = Quaternion.Lerp(startRot, slot.rotation, smoothT);
            
            yield return null;
        }

        // Final setup
        item.transform.SetParent(slot);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        
        slotItemMap[slot] = item;
        item.SetInventoryState(true, slot);
    }


    // Retrieve item from inventory


    public void RetrieveItemViaHand(InventoryItem item)
    {
        if (item.CurrentSlot != null)
        {
            Debug.Log("Retrieving item via hand: " + item.name);
            slotItemMap.Remove(item.CurrentSlot);
            item.transform.SetParent(null);
            item.SetInventoryState(false, null);
        }
    }

    public void RetrieveItemViaWand(InventoryItem item, Transform wandTransform)
    {
        if (item.CurrentSlot != null)
        {   
            Debug.Log("Retrieving item via wand: " + item.name);
            Transform slot = item.CurrentSlot;
            slotItemMap.Remove(slot);
            
            if (wandTransform != null)
            {   
                Debug.Log("If block");
                Vector3 retrievalPosition = retrievePosition.transform.position;
                StartCoroutine(SmoothRetrieveItem(item, retrievalPosition));
                
                
            }
            else
            {
                Debug.Log("Else block");
                item.transform.SetParent(null);
                item.SetInventoryState(false, null);
            }
        }
    }

    private IEnumerator SmoothRetrieveItem(InventoryItem item, Vector3 targetPosition)
    {
        float elapsedTime = 0f;
        Vector3 startPos = item.transform.position;
        
        item.transform.SetParent(null);
        Debug.Log("unparented item: " + item.name);
        item.SetInventoryState(false, null);
        
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            float smoothT = Mathf.SmoothStep(0, 1, t);
            
            item.transform.position = Vector3.Lerp(startPos, targetPosition, smoothT);
            
            yield return null;
        }
    }

    public void UpdateItemsVisibility(bool isInventoryOpen)
    {
        foreach (var kvp in slotItemMap)
        {
            if (kvp.Value != null)
            {
                kvp.Value.gameObject.SetActive(isInventoryOpen);
            }
        }
    }

    public void UpdateInventoryParenting()
    {
        foreach (var kvp in slotItemMap)
        {
            InventoryItem item = kvp.Value;
            Transform slot = kvp.Key;

            if (item != null && slot != null)
            {
                item.transform.SetParent(slot);
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;
            }
        }
    }
}