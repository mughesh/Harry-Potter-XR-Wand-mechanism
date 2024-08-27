using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AttachToHip : MonoBehaviour
{
    [SerializeField] private Transform leftHipAttachmentPoint; // Left hip attachment point for the book
    [SerializeField] private Transform rightHipAttachmentPoint; // Right hip attachment point for the wand
    [SerializeField] private XRGrabInteractable grabInteractable; // The grab interactable component

    private Transform originalParent; // To store the original parent (hip attachment point)
    private Rigidbody rb;

    private void Start()
    {   
        
        rb = GetComponent<Rigidbody>();

        // Determine whether this is the book or wand and set the original parent accordingly
        if (this.gameObject.name == "Book")
        {
            originalParent = leftHipAttachmentPoint;
        }
        else if (this.gameObject.name == "Wand")
        {
            originalParent = rightHipAttachmentPoint;
        }

        // Initially set the object to the respective hip attachment point
        transform.position = originalParent.position;
        transform.rotation = originalParent.localRotation;
        transform.SetParent(originalParent);
        SetPhysicsEnabled(false);

        // Register event listeners for grabbing and releasing the object
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    // When the object is grabbed
    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // Detach from the hip and let it follow the controller
        transform.SetParent(null);
        SetPhysicsEnabled(true);
    }

    // When the object is released
    private void OnReleased(SelectExitEventArgs args)
    {
        // Reattach the object to the respective hip attachment point
        transform.position = originalParent.position;
        transform.rotation = originalParent.localRotation;
        transform.SetParent(originalParent);
        SetPhysicsEnabled(false);
    }

    private void OnDestroy()
    {
        // Unregister the event listeners to avoid memory leaks
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }
    
        // Enable or disable physics on the object
    private void SetPhysicsEnabled(bool enabled)
    {
        if (enabled)
        {
            rb.isKinematic = false;  // Enable physics
            rb.useGravity = true;     // Enable gravity
        }
        else
        {
            rb.isKinematic = true;  // Disable physics (no gravity, no external forces)
            rb.useGravity = false;  // Disable gravity
        }
    }
}
