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
    public SpellEvent OnSpellCast = new SpellEvent();

    private XRGrabInteractable grabbable;

    void Start()
    {
        grabbable = GetComponent<XRGrabInteractable>();
        grabbable.activated.AddListener(FireWand);
    }

    private void FireWand(ActivateEventArgs args)
    {
        if (isSpellActive)
        {
            OnSpellCast.Invoke(activeSpell);
            isSpellActive = false;
        }
        else
        {
            DefaultFire();
        }
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