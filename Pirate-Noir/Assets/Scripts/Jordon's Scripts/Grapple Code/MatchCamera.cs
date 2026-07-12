using UnityEngine;

public class MatchCamera : MonoBehaviour
{
    public Transform targetCamera;

    void LateUpdate()
    {
        if (targetCamera != null)
        {
            transform.rotation = targetCamera.rotation;
        }
    }
}
