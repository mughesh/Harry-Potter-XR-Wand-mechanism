using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class InventoryItem : MonoBehaviour
{
    public InventoryItemData itemData;
    private InventorySystem inventorySystem;
    private XRGrabInteractable grabInteractable;

    private void Start()
    {
        inventorySystem = FindObjectOfType<InventorySystem>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        }

        SetupInteractions();
    }

    private void SetupInteractions()
    {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // Handle grabbing logic if needed
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        // Handle releasing logic if needed
    }

    public void AddToInventory()
    {
        StartCoroutine(MoveToBookCoroutine());
    }

    private IEnumerator MoveToBookCoroutine()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = inventorySystem.bookTransform.position;
        float duration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        inventorySystem.AddItem(itemData);
        gameObject.SetActive(false);
    }
}