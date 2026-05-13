using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class TouchCameraDrag : MonoBehaviour
{
    public CinemachineThirdPersonFollow mierda;

    private float vertRot = 0f;

    bool interactable = false;

    public void OnTouch()
    {
        float mouseX = Input.GetAxis("Mouse X") * 1000f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * 1000f * Time.deltaTime;

        vertRot -= mouseY;
        vertRot = Mathf.Clamp(vertRot, -90f, 90f);

        transform.localRotation = Quaternion.Euler(vertRot, transform.localEulerAngles.y + mouseX, 0f);
    }

    void Update()
    {
        if (interactable)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            mierda.CameraDistance -= scroll * mierda.CameraDistance / 2;
        }
    }

    public void MouseIO(bool input)
    {
        interactable = input;
    }
}