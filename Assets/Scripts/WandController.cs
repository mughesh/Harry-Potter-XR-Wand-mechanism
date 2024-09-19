using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WandController : MonoBehaviour
{
    public Transform wandTip;
    public GameObject crosshairPrefab;
    public float maxDistance = 10f;
    public Transform hipAttachPoint;

    private XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;
    private SpellSystem spellSystem;
    private GameObject crosshairInstance;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        spellSystem = FindObjectOfType<SpellSystem>();
        if (spellSystem == null)
        {
            Debug.LogError("SpellSystem not found in the scene!");
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
            grabInteractable.activated.AddListener(CastSpell);
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

    public void CastSpell(ActivateEventArgs args)
    {   
        if (isGrabbed && spellSystem != null && spellSystem.CurrentSpell != null)
        {
            spellSystem.CastSpell(wandTip.position, wandTip.forward);
        }
        else
        {
            Debug.LogWarning("Cannot cast spell: " + 
                (spellSystem == null ? "SpellSystem not found. " : "") +
                (spellSystem != null && spellSystem.CurrentSpell == null ? "No spell selected. " : "") +
                (!isGrabbed ? "Wand not grabbed. " : ""));
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