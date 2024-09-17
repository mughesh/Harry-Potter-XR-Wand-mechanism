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
    private LineRenderer lineRenderer;

    // Debug variables
    public bool debugMode = true;
    public Color debugRayColor = Color.red;
    public float debugSphereRadius = 0.05f;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        spellSystem = FindObjectOfType<SpellSystem>();
        SetupInteractions();
        CreateCrosshair();
        //SetupLineRenderer();
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

    // void SetupLineRenderer()
    // {
    //     lineRenderer = gameObject.AddComponent<LineRenderer>();
    //     lineRenderer.startWidth = 0.01f;
    //     lineRenderer.endWidth = 0.01f;
    //     lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    //     lineRenderer.startColor = Color.yellow;
    //     lineRenderer.endColor = Color.yellow;
    //     lineRenderer.positionCount = 2;
    //     lineRenderer.enabled = false;
    // }

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
            Debug.Log($"Crosshair position: {hit.point}, Normal: {hit.normal}, Distance: {hit.distance}");

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

            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, wandTip.position);
                lineRenderer.SetPosition(1, hit.point);
            }
            else
            {
                Debug.LogWarning("LineRenderer is null!");
            }

            // Debug visualization
            if (debugMode)
            {
                Debug.DrawLine(wandTip.position, hit.point, debugRayColor);
                Debug.DrawRay(hit.point, hit.normal, Color.blue);
            }
        }
        else
        {
            Debug.Log("No hit detected");
            if (crosshairInstance != null) crosshairInstance.SetActive(false);
            if (lineRenderer != null) lineRenderer.enabled = false;

            // Debug visualization
            if (debugMode)
            {
                Debug.DrawRay(wandTip.position, wandTip.forward * maxDistance, debugRayColor);
            }
        }
    }

    void SetupInteractions()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
        else
        {
            Debug.LogError("XRGrabInteractable component not found on the Wand object.");
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        Debug.Log("Wand grabbed");
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        Debug.Log("Wand released");
        ReturnToHip();
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

    void OnDrawGizmos()
    {
        if (debugMode && wandTip != null)
        {
            Gizmos.color = debugRayColor;
            Gizmos.DrawRay(wandTip.position, wandTip.forward * maxDistance);
            Gizmos.DrawWireSphere(wandTip.position, debugSphereRadius);
        }
    }
}