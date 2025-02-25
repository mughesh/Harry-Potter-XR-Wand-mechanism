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
    public LayerMask interactableLayer;  // For General interactables eg. spells, interactable in scene objects
    public LayerMask inventoryItemLayer;    // For inventory items
    public LayerMask bookLayerMask;         // For book UI
    private LayerMask excludeRaycastLayerMask;     // combined layer masks
    private LayerMask crosshairInteractableLayer;
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
    private Coroutine activeSpellCoroutine;
    private bool isSpellActive = false;
    private bool isCastingSpell = false;  

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
        excludeRaycastLayerMask = inventoryItemLayer | bookLayerMask;

        crosshairInteractableLayer = interactableLayer | excludeRaycastLayerMask;


        // To explicitly exclude layers
        int excludeLayers = (1 << LayerMask.NameToLayer("Inventory slots")) | (1 << LayerMask.NameToLayer("Book controller"));
        excludeRaycastLayerMask &= ~excludeLayers;

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
        if (Physics.Raycast(wandTip.position, wandTip.forward, out currentRaycastHit, maxDistance, crosshairInteractableLayer))
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
        //spellSystem.ResetSpell();
        spellSystem.CleanupActiveSpell();
        ReturnToHip();
    }

    public void OnActivate(ActivateEventArgs args)
    {
        if (!isActivated && isGrabbed)
        {
            Debug.Log("wand activated");
            isActivated = true;
            HandleWandInteraction();
            // Debug.Log("crosshair hitting" + hit.collider.name);
        }
    }
    public void OnDeactivate(DeactivateEventArgs args)
    {
        isActivated = false;
        // Only stop the spell if it's a hold-type spell
        if (spellSystem != null && spellSystem.CurrentSpell != null)
        {
            if (spellSystem.CurrentSpell.triggerType == SpellTriggerType.Hold)
            {
                spellSystem.StopActiveSpell();
            }
        }
        isCastingSpell = false;  // Reset the casting flag
    }


    void HandleWandInteraction()
    {
        //Debug.Log("Handle wand interaction");
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
                    HandleInteractableInteraction(currentRaycastHit);
                }
                // Check for other interactables (like spell selection)
                else if (((1 << currentRaycastHit.collider.gameObject.layer) & crosshairInteractableLayer) != 0)
                {
                    CastSpell();
                }

            }
            else
            {
                Debug.Log("No raycast hit detected.");
                CastSpell();

            }
        }
    }

    void CastSpell()
    {
        if (spellSystem.CurrentSpell != null)
        {
            if (spellSystem.CurrentSpell.triggerType == SpellTriggerType.Press)
            {
                if (!isCastingSpell)  // Only cast if we're not already casting
                {
                    isCastingSpell = true;
                    spellSystem.CastSpell(spellSystem.CurrentSpell, wandTip.position, wandTip.forward);
                }
            }
            else if (spellSystem.CurrentSpell.triggerType == SpellTriggerType.Hold && !isSpellActive)
            {
                isSpellActive = true;
                activeSpellCoroutine = StartCoroutine(ContinuousSpellCast());
            }
        }
    }

    private IEnumerator ContinuousSpellCast()
    {
        while (isActivated && spellSystem.CurrentSpell != null)
        {
            if (spellSystem.CurrentSpell.triggerType == SpellTriggerType.Hold)
            {
                spellSystem.CastSpell(spellSystem.CurrentSpell, wandTip.position, wandTip.forward);
            }
            yield return null;
        }
        isSpellActive = false;
    }

    void HandleInventoryItemInteraction(RaycastHit hit)
    {
        if (inventorySystem == null)
        {
            Debug.LogError("InventorySystem reference missing in WandController!");
            return;
        }

        InventoryItem inventoryItem = hit.collider.GetComponentInParent<InventoryItem>();
        InventoryItem rootObject = inventoryItem.GetComponentInParent<InventoryItem>();
        Debug.Log("raycast hit : " + hit.collider.name);
        Debug.Log("inventory item : " + inventoryItem);
        Debug.Log("root object : " + rootObject);
        if (inventoryItem != null)
        {
            Debug.Log("Inventory item found: " + inventoryItem.name);
            if (inventoryItem.IsInSlot)
            {
                Debug.Log("wand controller - retrieval: " + inventoryItem.name);
                //inventoryItem.SetInventoryState(false, null);                   // --------
                inventorySystem.RetrieveItemViaWand(inventoryItem, transform);

            }
            else if (!inventoryItem.IsInSlot)
            {
                inventorySystem.AddItemViaWand(inventoryItem);
                //inventoryItem.SetInventoryState(true, transform);       // --------
                Debug.Log("Inventory item added: " + inventoryItem.name);
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