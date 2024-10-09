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

    private XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;
    private bool isActivated = false;
    private SpellSystem spellSystem;
    private GameObject crosshairInstance;
    private BookController bookController;
    public Transform retrievePosition;
    public float retrievalDistance = 1.5f;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        spellSystem = FindObjectOfType<SpellSystem>();
        if (spellSystem == null)
        {
            Debug.LogError("SpellSystem not found in the scene!");
        }

        bookController = FindObjectOfType<BookController>();
        if (bookController == null)
        {
            Debug.LogError("BookController not found in the scene!");
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
        if (!isActivated)
        {
            isActivated = true;
            if (isGrabbed)
            {
                RaycastHit hit;
                if (Physics.Raycast(wandTip.position, wandTip.forward, out hit, maxDistance, interactableLayerMask))
                {
                    // Check if we hit an interactable object
                    XRSimpleInteractable interactable = hit.collider.GetComponent<XRSimpleInteractable>();
                    if (interactable != null)
                    {
                        // Manually invoke the interaction
                        interactable.selectEntered.Invoke(new SelectEnterEventArgs());
                        Debug.Log("Interacting with: " + hit.collider.gameObject.name);
                    }
                    else
                    {
                        Debug.Log("Hit object is not interactable");
                    }
                }
                else if (spellSystem != null && spellSystem.CurrentSpell != null)
                {
                    CastSpell(args);
                    Debug.Log("Casting spell: " + spellSystem.CurrentSpell.spellName);
                }
                
        //Inventory --------------------------------------------------------------------------------------------------
                if (Physics.Raycast(wandTip.position, wandTip.forward, out hit, maxDistance, inventoryItemLayer))
                {
                    InventoryItem inventoryItem = hit.collider.GetComponent<InventoryItem>();
                    if (inventoryItem != null)
                    {
                        if (inventoryItem.transform.parent == null) // Item is in the world
                        {
                            if (bookController.AddItemToInventory(inventoryItem))
                            {
                                Debug.Log("Item added to inventory: " + inventoryItem.itemData.itemName);
                            }
                            else
                            {
                                Debug.Log("Inventory is full!");
                            }
                        }
                        else // Item is in the inventory
                        {
                            Vector3 retrievalPosition = Camera.main.transform.position + Camera.main.transform.forward * retrievalDistance;
                            inventoryItem.RetrieveFromInventory(retrievalPosition);
                            Debug.Log("Item retrieved from inventory: " + inventoryItem.itemData.itemName);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Cannot cast spell: " + 
                        (spellSystem == null ? "SpellSystem not found. " : "") +
                        (spellSystem != null && spellSystem.CurrentSpell == null ? "No spell selected. " : "") +
                        (!isGrabbed ? "Wand not grabbed. " : ""));
                }
            }
        }
    }

        public void OnDeactivate(DeactivateEventArgs args)
    {
        isActivated = false;
    }


    public void CastSpell(ActivateEventArgs args)
    {   
        spellSystem.CastSpell(wandTip.position, wandTip.forward);
        Debug.Log("Casting spell method: " + spellSystem.CurrentSpell.spellName);
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