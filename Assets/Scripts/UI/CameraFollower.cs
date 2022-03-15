using System.Collections;
using UnityEngine;

public class CameraFollower : TargetFollower
{
    private const int maxAttempts = 50;
    private int attempts = 0;

    private void Awake()
    {
        enabled = false;
        StartCoroutine(FindCamera());
    }

    private IEnumerator FindCamera()
    {
        attempts = 0;
        while (target == null || attempts >= maxAttempts)
        {
            yield return new WaitForEndOfFrame();
            target = Camera.main.transform;
            attempts++;
        }

        if (attempts >= maxAttempts)
        {
            Debug.LogWarning("No main camera found, can't follow camera!");
        }
        else
        {
            transform.position = GetDesiredPosition();
            transform.rotation = target.rotation;
            enabled = true;
        }
    }   
}
