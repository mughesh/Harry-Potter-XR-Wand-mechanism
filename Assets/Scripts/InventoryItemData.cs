using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory Item", menuName = "Inventory/Inventory Item Data")]
public class InventoryItemData : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite inventorySprite; // 2D sprite for the book
    public GameObject worldPrefab; // 3D object prefab
}