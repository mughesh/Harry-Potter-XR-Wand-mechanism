using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class WandController : MonoBehaviour
{
    public Transform wandTip;
    public GameObject crosshairPrefab;
    public float maxDistance = 10f;
    public Transform hipAttachPoint;
    public LayerMask interactableLayerMask;  // For General interactables eg. spells, interactable in scene objects
    public LayerMask inventoryItemLayer;    // For inventory items
    public LayerMask bookLayerMask;         // For book UI
    private LayerMask raycastLayerMask;     // combined layer masks
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
    private RaycastHit currentRaycastHit;
    private GameObject previousHitObject = null;

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

            // Combine the layer masks but exclude the layers to ignore
        raycastLayerMask = interactableLayerMask | inventoryItemLayer | bookLayerMask;
        
         // To explicitly exclude layers
        int excludeLayers = (1 << LayerMask.NameToLayer("Inventory slots")) | (1 << LayerMask.NameToLayer("Book controller"));
        raycastLayerMask &= ~excludeLayers;

        SetupInteractions();
        CreateCrosshair();
     
    }

    void Update()
    {
        PerformRaycast();
        UpdateCrosshair();
    }

    void PerformRaycast()
    {
        if (Physics.Raycast(wandTip.position, wandTip.forward, out currentRaycastHit, maxDistance, raycastLayerMask))
        {
            if (currentRaycastHit.collider.gameObject != previousHitObject)
            {
                previousHitObject = currentRaycastHit.collider.gameObject;
            }
        }
        else
        {
            previousHitObject = null;
        }
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

        if (currentRaycastHit.collider != null)
        {
            if (crosshairInstance != null)
            {
                crosshairInstance.SetActive(true);
                crosshairInstance.transform.position = currentRaycastHit.point;
                crosshairInstance.transform.rotation = Quaternion.LookRotation(-currentRaycastHit.normal);
                //Debug.Log("crosshair hitting" + currentRaycastHit.collider.name);
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
           // Debug.Log("crosshair hitting" + hit.collider.name);
        }
    }
    public void OnDeactivate(DeactivateEventArgs args)
    {
        isActivated = false;
    }


    void HandleWandInteraction()
    {
        if (isActivated && isGrabbed)
        {
            if (currentRaycastHit.collider != null)
            {
                // Use the stored currentRaycastHit in the interaction handling methods
                if (((1 << currentRaycastHit.collider.gameObject.layer) & inventoryItemLayer) != 0)
                {
                    HandleInventoryItemInteraction(currentRaycastHit);
                }
                // Check for book interaction
                else if (((1 << currentRaycastHit.collider.gameObject.layer) & bookLayerMask) != 0)
                {
                    HandleBookInteraction(currentRaycastHit);
                }
                // Check for other interactables (like spell selection)
                else if (((1 << currentRaycastHit.collider.gameObject.layer) & interactableLayerMask) != 0)
                {
                    HandleInteractableInteraction(currentRaycastHit);
                }
                // If none of the above, try to cast spell
                else
                {
                    CastSpell();
                }
            }
            else
            {
                // Cast the spell
                if (spellSystem.CurrentSpell != null)
                {
                    if (spellSystem.CurrentSpell.triggerType == SpellTriggerType.Press)
                    {
                        // Cast the spell immediately
                        spellSystem.CastSpell(spellSystem.CurrentSpell, wandTip.position, wandTip.forward);
                    }
                    else if (spellSystem.CurrentSpell.triggerType == SpellTriggerType.Hold)
                    {
                        // Start casting the spell and continue until deactivated
                        StartCoroutine(CastHeldSpell(spellSystem.CurrentSpell, wandTip.position, wandTip.forward));
                    }
                }
            }
        }
    }

IEnumerator CastHeldSpell(SpellData spell, Vector3 startPosition, Vector3 direction)
{
    while (isActivated)
    {
        // Cast the spell (based on its type)
        spellSystem.CastSpell(spell, startPosition, direction);
        yield return null;
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
        //Debug.Log("raycast hit : " + hit.collider.name);
        if (inventoryItem != null)
        {
            if (inventoryItem.IsInSlot)
            {
                Debug.Log("wand controller - retrieval: " + inventoryItem.name);
                //inventoryItem.SetInventoryState(false, null);                   // --------
                inventorySystem.RetrieveItemViaWand(inventoryItem, transform);
                
            }
            else
            {
                inventorySystem.AddItemViaWand(inventoryItem);
                //inventoryItem.SetInventoryState(true, transform);       // --------
                //Debug.Log("Inventory item added: " + inventoryItem.name);
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
            spellSystem.CastSpell(spellSystem.CurrentSpell,wandTip.position, wandTip.forward);
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