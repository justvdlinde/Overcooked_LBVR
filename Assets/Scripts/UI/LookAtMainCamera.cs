using UnityEngine;

public class LookAtMainCamera : MonoBehaviour
{
    private void Update()
    {
        transform.LookAt(Camera.main.transform, -Camera.main.transform.up);
    }
}
