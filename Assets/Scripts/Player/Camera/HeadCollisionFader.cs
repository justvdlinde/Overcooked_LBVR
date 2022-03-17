using UnityEngine;

/// <summary>
/// Fades out player screen based on distance since last collision hit, so players cant look through objects.
/// </summary>
public class HeadCollisionFader : MonoBehaviour
{
    [SerializeField] private PlayerCollisionDetector colDetector = null;
    [SerializeField] private ScreenFader screenFader = null;
    [SerializeField] private float maxDistanceFade = 0.1f;

    private Vector3 lastHitPoint;
    private float distance;

    protected virtual void OnTriggerEnter(Collider collider)
    {
        if (!colDetector.IsColliding)
        {
            lastHitPoint = transform.position;
        }
    }

    protected virtual void OnTriggerStay(Collider collider)
    {
        distance = Vector3.Distance(transform.position, lastHitPoint);
        //screenFader.SetFadeLevel(distance / maxDistanceFade);
    }

    protected virtual void OnTriggerExit(Collider collider)
    {
        if (colDetector.Collisions.Count == 0)
        {
            screenFader.FadeOut();
        }
    }
}
