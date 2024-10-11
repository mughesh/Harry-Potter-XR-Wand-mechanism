using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class InventoryItem : MonoBehaviour
{
    public InventoryItemData itemData;
    private XRGrabInteractable grabInteractable;
    private Vector3 originalScale;
    private Transform originalParent;
     private XRSocketInteractor currentSocket;
     private bool isInBookCollider = false;
    
    [SerializeField] private float scaleDuration = 0.5f;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float inventoryScale = 0.2f;

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        originalScale = transform.localScale;
        originalParent = transform.parent;
        
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    public void AddToInventory(Transform slot)
    {
        StartCoroutine(AddToInventoryCoroutine(slot));
    }

   private IEnumerator AddToInventoryCoroutine(Transform slot)
    {
        Vector3 startPosition = transform.position;
        
        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            transform.position = Vector3.Lerp(startPosition, slot.position, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.SetParent(slot);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        currentSocket = slot.GetComponent<XRSocketInteractor>();
    }

    public void RetrieveFromInventory(Vector3 targetPosition)
    {
        StartCoroutine(RetrieveFromInventoryCoroutine(targetPosition));
    }

    private IEnumerator RetrieveFromInventoryCoroutine(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        
        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.SetParent(null);
        StartCoroutine(ScaleToOriginal());
    }


    private IEnumerator ScaleDown()
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = originalScale * inventoryScale;
        
        float elapsedTime = 0f;
        while (elapsedTime < scaleDuration)
        {
            float t = elapsedTime / scaleDuration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

   private IEnumerator ScaleToOriginal()
    {
        Vector3 startScale = transform.localScale;
        
        float elapsedTime = 0f;
        while (elapsedTime < scaleDuration)
        {
            float t = elapsedTime / scaleDuration;
            transform.localScale = Vector3.Lerp(startScale, originalScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        StopAllCoroutines();
        transform.localScale = originalScale;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (isInBookCollider)
        {
            BookController bookController = FindObjectOfType<BookController>();
            if (bookController != null)
            {
                Transform availableSlot = bookController.GetAvailableSlot();
                if (availableSlot != null)
                {
                    AddToInventory(availableSlot);
                }
            }
        }
        else
        {
            StartCoroutine(ScaleToOriginal());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BookCollider"))
        {
            isInBookCollider = true;
            StartCoroutine(ScaleDown());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("BookCollider"))
        {
            isInBookCollider = false;
            if (!grabInteractable.isSelected)
            {
                StartCoroutine(ScaleToOriginal());
            }
        }
    }

    // public void OnSelectEntered(SelectEnterEventArgs args)
    // {
    //     if (args.interactorObject is XRSocketInteractor socketInteractor)
    //     {
    //         currentSocket = socketInteractor;
    //         transform.SetParent(socketInteractor.transform);
    //     }
    // }

    // public void OnSelectExited(SelectExitEventArgs args)
    // {
    //     if (args.interactorObject is XRSocketInteractor)
    //     {
    //         // Only unparent if it's not being grabbed by a hand
    //         if (!(args.interactorObject is XRDirectInteractor))
    //         {
    //             transform.SetParent(null);
    //             currentSocket = null;
    //         }
    //     }
    // }
}