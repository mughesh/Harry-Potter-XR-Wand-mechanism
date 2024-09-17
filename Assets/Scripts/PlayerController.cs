using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerController : MonoBehaviour
{
    public Transform leftHipPosition;
    public Transform rightHipPosition;
    public GameObject bookObject;
    public GameObject wandObject;
    public XRDirectInteractor leftHand;
    public XRDirectInteractor rightHand;

    void Start()
    {
        if (bookObject != null)
        {
            bookObject.transform.SetParent(leftHipPosition);
            bookObject.transform.localPosition = Vector3.zero;
            bookObject.transform.localRotation = Quaternion.identity;
        }

        if (wandObject != null)
        {
            wandObject.transform.SetParent(rightHipPosition);
            wandObject.transform.localPosition = Vector3.zero;
            wandObject.transform.localRotation = Quaternion.identity;
        }
    }
}
