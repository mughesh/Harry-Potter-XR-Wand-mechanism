using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WandController : MonoBehaviour
{
    public Transform wandTip;
    public GameObject crosshairPrefab;
    public float maxDistance = 10f;
    public Transform hipAttachPoint;
    public LayerMask interactableLayerMask;
    public LayerMask inventoryItemLayer;
    public LayerMask bookLayerMask;

    private XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;
    private bool isActivated = false;
    private SpellSystem spellSystem;
     [SerializeField] private InventorySystem inventorySystem;
    private GameObject crosshairInstance;
    private BookController bookController;
    public Transform retrievePosition;
    public float retrievalDistance = 1.5f;
    public Transform characterController;


    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        spellSystem = FindObjectOfType<SpellSystem>();
        bookController = FindObjectOfType<BookController>();
        inventorySystem = FindObjectOfType<InventorySystem>();

        if (grabInteractable == null)
        {
            Debug.LogError("XRGrabInteractable component not found on the Wand object!");
        }
        if (spellSystem == null)
        {
            Debug.LogError("SpellSystem not found in the scene!");
        }

        if (bookController == null)
        {
            Debug.LogError("BookController not found in the scene!");
        }

        if (inventorySystem == null)
        {
            Debug.LogError("InventorySystem not found!");
        }
        SetupInteractions();
        CreateCrosshair();
    }

    void Update()
    {
        UpdateCrosshair();
    }

    void CreateCrosshair()
    {
        if (crosshairPrefab != null)
        {
            crosshairInstance = Instantiate(crosshairPrefab, Vector3.zero, Quaternion.identity);
            crosshairInstance.SetActive(false);
        }
        else
        {
            Debug.LogError("Crosshair prefab is not assigned to WandController.");
        }
    }

    void UpdateCrosshair()
    {
        if (wandTip == null)
        {
            Debug.LogError("WandTip is not assigned!");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(wandTip.position, wandTip.forward, out hit, maxDistance))
        {
            if (crosshairInstance != null)
            {
                crosshairInstance.SetActive(true);
                crosshairInstance.transform.position = hit.point;
                crosshairInstance.transform.rotation = Quaternion.LookRotation(-hit.normal);
            }
            else
            {
                Debug.LogWarning("Crosshair instance is null!");
            }
        }
        else
        {
            if (crosshairInstance != null) crosshairInstance.SetActive(false);
        }
    }

    void SetupInteractions()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
            grabInteractable.activated.AddListener(OnActivate);
            grabInteractable.deactivated.AddListener(OnDeactivate);
        }
        else
        {
            Debug.LogError("XRGrabInteractable component not found on the Wand object.");
        }
    }

    public void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    public void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        ReturnToHip();
    }

    public void OnActivate(ActivateEventArgs args)
    {
        if (!isActivated && isGrabbed)
        {
            isActivated = true;
            HandleWandInteraction();
        }
    }
    public void OnDeactivate(DeactivateEventArgs args)
    {
        isActivated = false;
    }


    void HandleWandInteraction()
    {
        RaycastHit hit;
        if (Physics.Raycast(wandTip.position, wandTip.forward, out hit, maxDistance))
        {
            // Check for inventory item
            if (((1 << hit.collider.gameObject.layer) & inventoryItemLayer) != 0)
            {
                HandleInventoryItemInteraction(hit);
            }
            // Check for book interaction
            else if (((1 << hit.collider.gameObject.layer) & bookLayerMask) != 0)
            {
                HandleBookInteraction(hit);
            }
            // Check for other interactables (like spell selection)
            else if (((1 << hit.collider.gameObject.layer) & interactableLayerMask) != 0)
            {
                HandleInteractableInteraction(hit);
            }
            // If none of the above, try to cast spell
            else
            {
                CastSpell();
            }
        }
        else
        {
            // If nothing was hit, try to cast spell
            CastSpell();
        }
    }

void HandleInventoryItemInteraction(RaycastHit hit)
    {
        if (inventorySystem == null)
        {
            Debug.LogError("InventorySystem reference missing in WandController!");
            return;
        }

        InventoryItem inventoryItem = hit.collider.GetComponent<InventoryItem>();
        if (inventoryItem != null)
        {
            if (inventoryItem.IsInSlot)
            {
                inventorySystem.RetrieveItemViaWand(inventoryItem, transform);
            }
            else
            {
                inventorySystem.AddItemViaWand(inventoryItem);
            }
        }
    }

    void HandleBookInteraction(RaycastHit hit)
    {
        // Implement book-specific interactions here
        // For example, turning pages or selecting spells
        XRSimpleInteractable interactable = hit.collider.GetComponent<XRSimpleInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.Invoke(new SelectEnterEventArgs());
        }
    }

    void HandleInteractableInteraction(RaycastHit hit)
    {
        XRSimpleInteractable interactable = hit.collider.GetComponent<XRSimpleInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.Invoke(new SelectEnterEventArgs());
        }
    }

    void CastSpell()
    {
        if (spellSystem != null && spellSystem.CurrentSpell != null)
        {
            spellSystem.CastSpell(wandTip.position, wandTip.forward);
        }
        else
        {
            Debug.Log("No spell selected or spell system not found.");
        }
    }
    
    void ReturnToHip()
    {
        if (hipAttachPoint != null)
        {
            transform.SetParent(hipAttachPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
}