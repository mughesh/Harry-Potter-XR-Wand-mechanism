using UnityEngine;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    public List<InventoryItemData> inventoryItems;

    public void AddItem(InventoryItemData item)
    {
        inventoryItems.Add(item);
        // Update UI or book display
    }

    public void RemoveItem(InventoryItemData item)
    {
        inventoryItems.Remove(item);
        // Update UI or book display
    }

    public void SpawnItem(InventoryItemData item, Vector3 position)
    {
        // Instantiate 3D prefab at position
        Instantiate(item.worldPrefab, position, Quaternion.identity);
        RemoveItem(item);
    }
}
