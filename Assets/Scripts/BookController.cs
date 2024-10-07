using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BookController : MonoBehaviour
{
    public GameObject[] bookmarks;
    public GameObject[] spellPages;
    public GameObject[] inventoryPages;
    public Transform[] inventorySlots;
    public GameObject leftArrow;
    public GameObject rightArrow;
    public Transform hipAttachPoint;
    private Vector3[] originalBookmarkScales;

    private XRGrabInteractable grabInteractable;
    private int currentSegment = 0; // 0 for spells, 1 for inventory
    private int currentPage = 0;
    public XRDirectInteractor leftHandInteractor;


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

    // Restrict book grabbing to left hand only
        grabInteractable.interactionLayers = 1 << leftHandInteractor.gameObject.layer;
   
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
    
    public bool AddItemToInventory(InventoryItem item)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].childCount == 0)
            {
                item.AddToInventory(inventorySlots[i]);
                return true;
            }
        }
        return false; // Inventory is full
    }

    void UpdateBookDisplay()
    {
    // Always show all bookmarks
    for (int i = 0; i < bookmarks.Length; i++)
    {
        bookmarks[i].SetActive(true);  // Ensure bookmarks are always active

        // Scale the current segment bookmark slightly larger to signify it
        if (i == currentSegment)
        {
            bookmarks[i].transform.localScale = originalBookmarkScales[i] * 1.2f;  // Increase size by 20%
        }
        else
        {
            bookmarks[i].transform.localScale = originalBookmarkScales[i];  // Reset to original size
        }
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

        // Update arrows visibility
        UpdateArrowsVisibility();
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
        // Book grabbed logic
        Debug.Log("Book grabbed");
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
}