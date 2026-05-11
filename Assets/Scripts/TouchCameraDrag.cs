using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class TouchCameraDrag : MonoBehaviour
{
    public CinemachineThirdPersonFollow mierda;
    public void OnTouch()
    {
        Vector3 rot = new Vector3(Input.GetAxis("Mouse Y") * -0.1f, Input.GetAxis("Mouse X") * 1000, 0);
        transform.Rotate(rot * Time.deltaTime);
        mierda.CameraDistance += rot.x;
    }
}