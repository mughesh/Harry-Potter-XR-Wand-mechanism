using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class WandController : MonoBehaviour
{
    public GameObject defaultProjectilePrefab;
    public Transform spawnPoint;
    public float fireSpeed = 20f;

    private bool isSpellActive = false;
    private string activeSpell = "";

    [System.Serializable]
    public class SpellEvent : UnityEvent<string> { }

    public SpellEvent OnSpellSelected = new SpellEvent();
    public SpellEvent OnTriggerPressed = new SpellEvent();
    public SpellEvent OnTriggerHeld = new SpellEvent();
    public SpellEvent OnTriggerReleased = new SpellEvent();

    private XRGrabInteractable grabbable;

    private bool isTriggerHeld = false;

    void Start()
    {
        grabbable = GetComponent<XRGrabInteractable>();
        grabbable.activated.AddListener(HandleTriggerPressed);
        grabbable.deactivated.AddListener(HandleTriggerReleased);
    }

    void Update()
    {
        // Check for trigger hold
        if (isTriggerHeld && isSpellActive)
        {
            OnTriggerHeld.Invoke(activeSpell);
        }
    }

    private void HandleTriggerPressed(ActivateEventArgs args)
    {
        isTriggerHeld = true;

        if (isSpellActive)
        {
            OnTriggerPressed.Invoke(activeSpell);
        }
        else
        {
            DefaultFire();
        }
    }

    private void HandleTriggerReleased(DeactivateEventArgs args)
    {
        if (isSpellActive)
        {
            OnTriggerReleased.Invoke(activeSpell);
        }

        isTriggerHeld = false;
    }

    private void DefaultFire()
    {
        GameObject spawnedProjectile = Instantiate(defaultProjectilePrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedProjectile.GetComponent<Rigidbody>().velocity = spawnPoint.forward * fireSpeed;
        Destroy(spawnedProjectile, 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Spell"))
        {
            string spellName = other.gameObject.name;
            SelectSpell(spellName);
        }
    }

    private void SelectSpell(string spellName)
    {
        isSpellActive = true;
        activeSpell = spellName;
        OnSpellSelected.Invoke(spellName);
    }
}
