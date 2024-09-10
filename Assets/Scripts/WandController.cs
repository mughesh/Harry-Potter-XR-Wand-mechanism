using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WandController : MonoBehaviour
{
    public Transform spawnPoint;
    public float maxAimDistance = 10f;
    public GameObject crosshairPrefab;

    public delegate void SpellEventHandler(string spellName);
    public event SpellEventHandler OnSpellAimed;
    public event SpellEventHandler OnSpellSelected;
    public event SpellEventHandler OnTriggerPressed;
    public event SpellEventHandler OnTriggerHeld;
    public event SpellEventHandler OnTriggerReleased;

    private XRGrabInteractable grabbable;
    private bool isTriggerHeld = false;
    private string currentSpell = "Default";
    private string aimedSpell = "";
    private GameObject crosshair;

    void Start()
    {
        grabbable = GetComponent<XRGrabInteractable>();
        grabbable.activated.AddListener(HandleTriggerPressed);
        grabbable.deactivated.AddListener(HandleTriggerReleased);

        crosshair = Instantiate(crosshairPrefab, spawnPoint.position, Quaternion.identity);
        crosshair.SetActive(false);
    }

    void Update()
    {
        UpdateAim();

        if (isTriggerHeld)
        {
            OnTriggerHeld?.Invoke(currentSpell);
        }
    }

    private void UpdateAim()
    {
        RaycastHit hit;
        if (Physics.Raycast(spawnPoint.position, spawnPoint.forward, out hit, maxAimDistance))
        {
            crosshair.SetActive(true);
            crosshair.transform.position = hit.point;

            if (hit.collider.CompareTag("Spell"))
            {
                aimedSpell = hit.collider.gameObject.name;
                OnSpellAimed?.Invoke(aimedSpell);
            }
            else
            {
                aimedSpell = "";
                OnSpellAimed?.Invoke("");
            }
        }
        else
        {
            crosshair.SetActive(false);
            aimedSpell = "";
            OnSpellAimed?.Invoke("");
        }
    }

    private void HandleTriggerPressed(ActivateEventArgs args)
    {
        isTriggerHeld = true;
        if (aimedSpell != "")
        {
            currentSpell = aimedSpell;
            OnSpellSelected?.Invoke(currentSpell);
            Debug.Log("Selected " + currentSpell);
        }
        else
        {
            OnTriggerPressed?.Invoke(currentSpell);
            //Debug.Log("Pressed " + currentSpell);
        }
    }

    private void HandleTriggerReleased(DeactivateEventArgs args)
    {
        isTriggerHeld = false;
        OnTriggerReleased?.Invoke(currentSpell);
    }
}