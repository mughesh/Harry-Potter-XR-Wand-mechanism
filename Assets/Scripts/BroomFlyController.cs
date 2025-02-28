using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Collections;

public class BroomFlyController : MonoBehaviour
{
    [Header("Broom References")]
    [SerializeField] private XRGrabInteractable broomGrabInteractable;
    [SerializeField] private Transform broomForwardReference;
    [SerializeField] private InventoryItem broomInventoryItem;
    
    [Header("Locomotion References")]
    [SerializeField] private ActionBasedContinuousMoveProvider moveProvider;
    [SerializeField] private InputActionReference flyToggleAction;
    
    [Header("Flying Settings")]
    [SerializeField] private float normalMoveSpeed = 2.5f;
    [SerializeField] private float flyingMoveSpeed = 5f;
    [SerializeField] private float minFlyingAngle = 30f; // Broom must be angled at least this much to fly
    [SerializeField] private float wobblingAmount = 0.15f;
    [SerializeField] private float wobblingSpeed = 2f;
    
    // Internal state tracking
    private bool isFlying = false;
    private bool isFlyModeEnabled = false;
    private bool isGrabbed = false;
    private Transform xrOriginTransform;
    private Coroutine wobblingCoroutine;
    private AudioSource flyingSoundSource;
    
    private void Start()
    {
        if (broomGrabInteractable == null)
            broomGrabInteractable = GetComponent<XRGrabInteractable>();
            
        if (broomInventoryItem == null)
            broomInventoryItem = GetComponent<InventoryItem>();
            
        if (moveProvider != null)
        {
            // Cache the XR Origin transform for wobbling effect
            if (moveProvider.system != null && moveProvider.system.xrOrigin != null)
                xrOriginTransform = moveProvider.system.xrOrigin.transform;
        }
        
        // Set up grab events
        if (broomGrabInteractable != null)
        {
            broomGrabInteractable.selectEntered.AddListener(OnBroomGrabbed);
            broomGrabInteractable.selectExited.AddListener(OnBroomReleased);
        }
        
        // Set up fly toggle action
        if (flyToggleAction != null)
        {
            flyToggleAction.action.performed += OnFlyToggle;
            flyToggleAction.action.Enable();
        }
    }
    
    private void OnDestroy()
    {
        // Clean up listeners
        if (broomGrabInteractable != null)
        {
            broomGrabInteractable.selectEntered.RemoveListener(OnBroomGrabbed);
            broomGrabInteractable.selectExited.RemoveListener(OnBroomReleased);
        }
        
        if (flyToggleAction != null)
        {
            flyToggleAction.action.performed -= OnFlyToggle;
        }
    }
    
    private void Update()
    {
        // Only check flying conditions if fly mode is enabled and broom is grabbed
        if (isFlyModeEnabled && isGrabbed && !broomInventoryItem.IsInSlot)
        {
            UpdateFlyingState();
        }
        else if (isFlying)
        {
            // Make sure flying is disabled if conditions aren't met
            DisableFlying();
        }
    }
    
    private void UpdateFlyingState()
    {
        // Check if broom is angled properly for flying
        float broomAngle = Vector3.Angle(broomForwardReference.forward, Vector3.up);
        bool shouldFly = broomAngle >= minFlyingAngle;
        
        if (shouldFly && !isFlying)
        {
            EnableFlying();
        }
        else if (!shouldFly && isFlying)
        {
            DisableFlying();
        }
    }
    
    private void OnBroomGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        
        // If the broom was in the inventory, make sure it's fully out before allowing flight
        if (broomInventoryItem != null && !broomInventoryItem.IsInSlot)
        {
            // Check if already in fly mode and update flying state
            if (isFlyModeEnabled)
            {
                UpdateFlyingState();
            }
        }
    }
    
    private void OnBroomReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
        
        // Disable flying when broom is released
        if (isFlying)
        {
            DisableFlying();
        }
    }
    
    private void OnFlyToggle(InputAction.CallbackContext context)
    {
        // Only toggle if broom is grabbed and not in inventory
        if (isGrabbed && broomInventoryItem != null && !broomInventoryItem.IsInSlot)
        {
            isFlyModeEnabled = !isFlyModeEnabled;
            
            if (isFlyModeEnabled)
            {
                // Check if we should start flying immediately
                UpdateFlyingState();
            }
            else if (isFlying)
            {
                // Disable flying if turning off fly mode
                DisableFlying();
            }
            
            Debug.Log("Broom Fly Mode: " + (isFlyModeEnabled ? "Enabled" : "Disabled"));
        }
    }
    
    private void EnableFlying()
    {
        if (moveProvider == null) return;
        
        // Save current settings to restore later
        normalMoveSpeed = moveProvider.moveSpeed;
        
        // Configure movement provider for flying
        moveProvider.moveSpeed = flyingMoveSpeed;
        moveProvider.enableFly = true;
        moveProvider.useGravity = false;
        
        // Set the broom as the forward reference
        if (broomForwardReference != null)
        {
            var previousForwardSource = moveProvider.forwardSource;
            moveProvider.forwardSource = broomForwardReference;
        }
        
        isFlying = true;
        Debug.Log("Broom flying enabled!");
        
        // Play flying sound
        if (flyingSoundSource == null)
            flyingSoundSource = GetComponent<AudioSource>();
            
        if (flyingSoundSource != null && !flyingSoundSource.isPlaying)
            flyingSoundSource.Play();
        
        // Start wobbling effect
        if (wobblingCoroutine != null)
            StopCoroutine(wobblingCoroutine);
            
        if (xrOriginTransform != null)
            wobblingCoroutine = StartCoroutine(WobblingEffect());
    }
    
    private void DisableFlying()
    {
        if (moveProvider == null) return;
        
        // Restore standard movement settings
        moveProvider.moveSpeed = normalMoveSpeed;
        moveProvider.enableFly = false;
        moveProvider.useGravity = true;
        moveProvider.forwardSource = null; // Reset to default (camera)
        
        isFlying = false;
        Debug.Log("Broom flying disabled!");
        
        // Stop playing flying sound
        if (flyingSoundSource != null && flyingSoundSource.isPlaying)
            flyingSoundSource.Stop();
        
        // Stop wobbling effect
        if (wobblingCoroutine != null)
        {
            StopCoroutine(wobblingCoroutine);
            wobblingCoroutine = null;
            
            // Reset any position offset from wobbling
            if (xrOriginTransform != null)
            {
                Vector3 currentPos = xrOriginTransform.position;
                xrOriginTransform.position = new Vector3(currentPos.x, currentPos.y, currentPos.z);
            }
        }
    }
    
    private IEnumerator WobblingEffect()
    {
        float time = 0f;
        Vector3 initialPos = xrOriginTransform.position;
        
        while (isFlying)
        {
            time += Time.deltaTime;
            
            // Create a slight up and down movement based on sine waves
            float verticalOffset = Mathf.Sin(time * wobblingSpeed) * wobblingAmount;
            float horizontalOffset = Mathf.Sin(time * wobblingSpeed * 0.5f) * (wobblingAmount * 0.3f);
            
            // Apply the offset to the XR Origin
            Vector3 currentPos = xrOriginTransform.position;
            xrOriginTransform.position = new Vector3(
                currentPos.x + horizontalOffset,
                currentPos.y + verticalOffset,
                currentPos.z
            );
            
            yield return null;
        }
    }
}