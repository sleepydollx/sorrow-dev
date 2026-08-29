using UnityEngine;

public class DoorController : MonoBehaviour
{
    public bool isOpen = false;
    public float doorOpenAngle = 90f;
    public float doorCloseAngle = 0f;
    public float smoothSpeed = 3f;

    public void InteractDoor()
    {
        isOpen = !isOpen;
    }

    void Update()
    {
        float targetAngle = isOpen ? doorOpenAngle : doorCloseAngle;
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
    }
}