using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WandController : MonoBehaviour
{
    public Transform spawnPoint;
    public float maxAimDistance = 10f;
    public GameObject crosshairPrefab;

    private XRGrabInteractable grabbable;
    private GameObject crosshair;
    private SpellManager spellManager;

    void Start()
    {
        grabbable = GetComponent<XRGrabInteractable>();
        grabbable.activated.AddListener(HandleTriggerPressed);
        grabbable.deactivated.AddListener(HandleTriggerReleased);
        grabbable.selectEntered.AddListener(HandleGrabbed);
        grabbable.selectExited.AddListener(HandleReleased);

        crosshair = Instantiate(crosshairPrefab, spawnPoint.position, Quaternion.identity);
        crosshair.SetActive(false);

        spellManager = FindObjectOfType<SpellManager>();
    }

    void Update()
    {
        UpdateAim();
    }

    private void UpdateAim()
    {
        RaycastHit hit;
        if (Physics.Raycast(spawnPoint.position, spawnPoint.forward, out hit, maxAimDistance))
        {
            crosshair.SetActive(true);
            crosshair.transform.position = hit.point;
            spellManager.AimAt(hit.collider.gameObject);
        }
        else
        {
            crosshair.SetActive(false);
            spellManager.AimAt(null);
        }
    }

    private void HandleTriggerPressed(ActivateEventArgs args)
    {
        spellManager.TriggerPressed();
    }

    private void HandleTriggerReleased(DeactivateEventArgs args)
    {
        spellManager.TriggerReleased();
    }

    private void HandleGrabbed(SelectEnterEventArgs args)
    {
        spellManager.WandGrabbed();
    }

    private void HandleReleased(SelectExitEventArgs args)
    {
        spellManager.WandReleased();
    }
}