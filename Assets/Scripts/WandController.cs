using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class WandController : MonoBehaviour
{
    public Transform spawnPoint;

    [System.Serializable]
    public class SpellEvent : UnityEvent<string> { }

    public SpellEvent OnSpellSelected = new SpellEvent();
    public SpellEvent OnTriggerPressed = new SpellEvent();
    public SpellEvent OnTriggerHeld = new SpellEvent();
    public SpellEvent OnTriggerReleased = new SpellEvent();

    private XRGrabInteractable grabbable;
    private bool isTriggerHeld = false;
    private string currentSpell = "Default";

    void Start()
    {
        grabbable = GetComponent<XRGrabInteractable>();
        grabbable.activated.AddListener(HandleTriggerPressed);
        grabbable.deactivated.AddListener(HandleTriggerReleased);
    }

    void Update()
    {
        if (isTriggerHeld)
        {
            OnTriggerHeld.Invoke(currentSpell);
        }
    }

    private void HandleTriggerPressed(ActivateEventArgs args)
    {
        isTriggerHeld = true;
        OnTriggerPressed.Invoke(currentSpell);
    }

    private void HandleTriggerReleased(DeactivateEventArgs args)
    {
        isTriggerHeld = false;
        OnTriggerReleased.Invoke(currentSpell);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Spell"))
        {
            currentSpell = other.gameObject.name;
            OnSpellSelected.Invoke(currentSpell);
        }
    }

    void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(spawnPoint.position, spawnPoint.forward * 0.5f);
        }
    }
}