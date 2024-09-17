using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory Item", menuName = "Inventory Item Data")]
public class InventoryItemData : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite bookSprite;
    public GameObject worldPrefab;
}
