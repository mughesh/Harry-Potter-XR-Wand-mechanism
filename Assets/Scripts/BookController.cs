using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class BookController : MonoBehaviour
{
    public GameObject[] bookmarks;
    public GameObject[] spellPages;
    public GameObject[] inventoryPages;
    public GameObject leftArrow;
    public GameObject rightArrow;
    public Transform hipAttachPoint;
    private Vector3[] originalBookmarkScales;
    private int currentSegment = 0; // 0 for spells, 1 for inventory
    private int currentPage = 0;
    private XRGrabInteractable grabInteractable;
    public XRSocketInteractor[] inventorySlots;
    private int currentSlotIndex = 0;

    private List<InventoryItem> inventoryItems = new List<InventoryItem>();



    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
            
        }
        else
        {
            Debug.LogError("XRGrabInteractable component not found on the Book object.");
        }


   
    // Store the original scale for each bookmark
    originalBookmarkScales = new Vector3[bookmarks.Length];
    for (int i = 0; i < bookmarks.Length; i++)
    {
        originalBookmarkScales[i] = bookmarks[i].transform.localScale;
    }

        UpdateBookDisplay();
        SetupInteractables();
    
    }

     void SetupInteractables()
    {
        // Setup bookmarks
        for (int i = 0; i < bookmarks.Length; i++)
        {
            int index = i; // Capture the index for use in lambda
            XRSimpleInteractable interactable = bookmarks[i].GetComponent<XRSimpleInteractable>();
            if (interactable == null)
            {
                interactable = bookmarks[i].AddComponent<XRSimpleInteractable>();
            }
            interactable.selectEntered.AddListener((args) => OnBookmarkSelected(index));
        }

        // Setup arrows
        SetupArrow(leftArrow, false);
        SetupArrow(rightArrow, true);

        // Setup spell items
        SetupSpellItems();
    }

     void SetupArrow(GameObject arrow, bool isNext)
    {
        XRSimpleInteractable interactable = arrow.GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = arrow.AddComponent<XRSimpleInteractable>();
        }
        interactable.selectEntered.AddListener((args) => OnArrowSelected(isNext));
    }

    void SetupSpellItems()
    {
        foreach (var page in spellPages)
        {
            SpellItem[] spellItems = page.GetComponentsInChildren<SpellItem>();
            foreach (var spellItem in spellItems)
            {
                XRSimpleInteractable interactable = spellItem.gameObject.GetComponent<XRSimpleInteractable>();
                if (interactable == null)
                {
                    interactable = spellItem.gameObject.AddComponent<XRSimpleInteractable>();
                }
                interactable.selectEntered.AddListener((args) => spellItem.OnSpellSelected(args));
            }
        }
    }


    void UpdateBookDisplay()
    {
        // Update bookmarks
        for (int i = 0; i < bookmarks.Length; i++)
        {
            bookmarks[i].SetActive(true);
            bookmarks[i].transform.localScale = (i == currentSegment) 
                ? originalBookmarkScales[i] * 1.2f 
                : originalBookmarkScales[i];
        }

        // Update pages visibility
        for (int i = 0; i < spellPages.Length; i++)
        {
            spellPages[i].SetActive(currentSegment == 0 && i == currentPage);
        }
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            inventoryPages[i].SetActive(currentSegment == 1 && i == currentPage);
        }

        // Update inventory items visibility
        UpdateInventoryItemsVisibility();

        UpdateArrowsVisibility();
    }

    void UpdateInventoryItemsVisibility()
    {
        foreach (var item in inventoryItems)
        {
            if (item != null)
            {
                bool shouldBeVisible = currentSegment == 1;
                item.gameObject.SetActive(shouldBeVisible);
                
                // Ensure proper parenting
                if (shouldBeVisible && item.transform.parent != item.CurrentSlot)
                {
                    item.transform.SetParent(item.CurrentSlot);
                    item.transform.localPosition = Vector3.zero;
                    item.transform.localRotation = Quaternion.identity;
                }
            }
        }
    }

    void UpdateArrowsVisibility()
    {
        if (currentSegment == 0)
        {
            leftArrow.SetActive(currentPage > 0);
            rightArrow.SetActive(currentPage < spellPages.Length - 1);
        }
        else
        {
            leftArrow.SetActive(currentPage > 0);
            rightArrow.SetActive(currentPage < inventoryPages.Length - 1);
        }
    }

    public void OnGrab(SelectEnterEventArgs args)
    {


    }

    private bool IsLeftHandInteractor(IXRSelectInteractor interactor)
    {
        // Check the tag of the interactor's gameObject
        return interactor.transform.CompareTag("LeftHand");
    }

    public void OnRelease(SelectExitEventArgs args)
    {
         ReturnToHip();      
    }

    public void OnBookmarkSelected(int index)
    {
        Debug.Log("Bookmark selected: " + index);
        currentSegment = index;
        currentPage = 0;
        UpdateBookDisplay();
    }

    public void OnArrowSelected(bool isNextPage)
    {
        if (isNextPage)
        {
            NextPage();
            Debug.Log("Next page");
        }
        else
        {
            PreviousPage();
            Debug.Log("Previous page");
        }
    }

        void NextPage()
    {
        if (currentSegment == 0 && currentPage < spellPages.Length - 1)
        {
            currentPage++;
        }
        else if (currentSegment == 1 && currentPage < inventoryPages.Length - 1)
        {
            currentPage++;
        }
        UpdateBookDisplay();
    }

    void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateBookDisplay();
        }
    }

    public void ReturnToHip()
    {
        if (hipAttachPoint != null)
        {
            transform.SetParent(hipAttachPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }

    public Transform GetAvailableSlot()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            int index = (currentSlotIndex + i) % inventorySlots.Length;
            if (inventorySlots[index].interactablesSelected.Count == 0)
            {
                currentSlotIndex = (index + 1) % inventorySlots.Length;
                return inventorySlots[index].transform;
            }
        }
        return null;
    }


    public void AddItemToInventory(InventoryItem item)
    {
        Transform availableSlot = GetAvailableSlot();
        if (availableSlot != null)
        {
            item.AddToInventory(availableSlot);
            inventoryItems.Add(item);
            UpdateInventoryItemsVisibility();
        }
        else
        {
            Debug.Log("No available slots in the inventory.");
        }
    }
    public void RemoveItemFromInventory(InventoryItem item)
    {
        inventoryItems.Remove(item);
        UpdateInventoryItemsVisibility();
    }
}