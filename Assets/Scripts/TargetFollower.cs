using UnityEngine;

public class TargetFollower : MonoBehaviour
{
    [SerializeField] protected Transform target = null;
    [SerializeField] protected float distance = 2;
    [SerializeField] protected float followStrength = 7;

    protected virtual void Update()
    {
        transform.position = Vector3.Lerp(transform.position, GetDesiredPosition(), followStrength * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, followStrength * Time.deltaTime);
    }

    protected virtual Vector3 GetDesiredPosition()
    {
        return target.position + target.forward * distance;
    }
}
