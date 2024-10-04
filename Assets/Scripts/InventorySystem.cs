using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public List<InventorySlot> inventorySlots;
    public Transform bookTransform;

    private void Start()
    {
        // Populate inventorySlots list with all InventorySlot components in children
        inventorySlots = new List<InventorySlot>(GetComponentsInChildren<InventorySlot>());
    }

    public void AddItem(InventoryItemData item)
    {
        InventorySlot emptySlot = inventorySlots.Find(slot => slot.currentItem == null);
        if (emptySlot != null)
        {
            emptySlot.SetItem(item);
        }
        else
        {
            Debug.LogWarning("Inventory is full!");
        }
    }

    public void SpawnItem(InventoryItemData item, Vector3 position)
    {
        GameObject spawnedItem = Instantiate(item.worldPrefab, position, Quaternion.identity);
        InventoryItem inventoryItem = spawnedItem.GetComponent<InventoryItem>();
        if (inventoryItem == null)
        {
            inventoryItem = spawnedItem.AddComponent<InventoryItem>();
        }
        inventoryItem.itemData = item;
    }
}