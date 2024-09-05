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
    public string currentSpell = "Default";
    private string selectedSpell = "";

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
           // Debug.Log($"UPDATE METHOD Spell held: {currentSpell}");
        }
    }

    private void HandleTriggerPressed(ActivateEventArgs args)
    {
        isTriggerHeld = true;
        if (selectedSpell != "")
        {
            currentSpell = selectedSpell;
            OnTriggerPressed.Invoke(currentSpell);
        }
        else
        {
            OnTriggerPressed.Invoke("Default");
        }
    }

    private void HandleTriggerReleased(DeactivateEventArgs args)
    {
        isTriggerHeld = false;
        OnTriggerReleased.Invoke(currentSpell);
        currentSpell = "Default";
        selectedSpell = "";
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Spell"))
        {
            selectedSpell = other.gameObject.name;
            OnSpellSelected.Invoke(selectedSpell);
            //Debug.Log($"TRIGGER STAY Spell selected: {selectedSpell}");
        }
    }

    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag("Spell") && other.gameObject.name == selectedSpell)
    //     {
    //         selectedSpell = "";
    //     }
    // }

    void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(spawnPoint.position, spawnPoint.forward * 0.5f);
        }
    }
}