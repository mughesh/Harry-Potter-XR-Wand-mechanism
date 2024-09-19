using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BookController : MonoBehaviour
{
    public GameObject[] bookmarks;
    public GameObject[] spellPages;
    public GameObject[] inventoryPages;
    public GameObject leftArrow;
    public GameObject rightArrow;
    public Transform hipAttachPoint;

    private XRGrabInteractable grabInteractable;
    private int currentSegment = 0; // 0 for spells, 1 for inventory
    private int currentPage = 0;

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

        UpdateBookDisplay();
    }

    void UpdateBookDisplay()
    {
        // Update bookmarks visibility
        bookmarks[0].SetActive(currentSegment == 0);
        bookmarks[1].SetActive(currentSegment == 1);

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

    void SelectSegment(int index)
    {
        currentSegment = index;
        currentPage = 0;
        UpdateBookDisplay();
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

    public void OnGrab(SelectEnterEventArgs args)
    {
        // Book grabbed logic
    }

    public void OnRelease(SelectExitEventArgs args)
    {
        ReturnToHip();
    }

    public void OnBookmarkSelected(int index)
    {
        SelectSegment(index);
        Debug.Log("Selected bookmark: " + index);
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