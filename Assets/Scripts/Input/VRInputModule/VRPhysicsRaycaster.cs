using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Simple event system using physics raycasts. Very closely based on UnityEngine.EventSystems.PhysicsRaycaster
/// </summary>
public class VRPhysicsRaycaster : BaseRaycaster
{
    public override Camera eventCamera => eventCameraReference;
    public Camera eventCameraReference = null;

    /// <summary>
    /// Const to use for clarity when no event mask is set
    /// </summary>
    protected const int kNoEventMaskSet = -1;

    /// <summary>
    /// Layer mask used to filter events. Always combined with the camera's culling mask if a camera is used.
    /// </summary>
    [SerializeField] protected LayerMask m_EventMask = kNoEventMaskSet;

    /// <summary>
    /// Depth used to determine the order of event processing.
    /// </summary>
    public virtual int depth
    {
        get { return (eventCamera != null) ? (int)eventCamera.depth : 0xFFFFFF; }
    }

    public int sortOrder = 0;
    public override int sortOrderPriority
    {
        get
        {
            return sortOrder;
        }
    }

    /// <summary>
    /// Event mask used to determine which objects will receive events.
    /// </summary>
    public int FinalEventMask
    {
        get { return (eventCamera != null) ? eventCamera.cullingMask & m_EventMask : kNoEventMaskSet; }
    }

    /// <summary>
    /// Layer mask used to filter events. Always combined with the camera's culling mask if a camera is used.
    /// </summary>
    public LayerMask EventMask
    {
        get { return m_EventMask; }
        set { m_EventMask = value; }
    }

    /// <summary>
    /// Perform a raycast using the worldSpaceRay in eventData.
    /// </summary>
    /// <param name="eventData"></param>
    /// <param name="resultAppendList"></param>
    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        if (eventCamera == null)
            return;

        if (!eventData.IsVRPointer())
            return;

        Ray ray = eventData.GetRay();
        float dist = eventCamera.farClipPlane - eventCamera.nearClipPlane;
        RaycastHit[] hits = Physics.RaycastAll(ray, dist, FinalEventMask);

        if (hits.Length > 1)
            System.Array.Sort(hits, (r1, r2) => r1.distance.CompareTo(r2.distance));

        if (hits.Length != 0)
        {
            for (int b = 0, bmax = hits.Length; b < bmax; ++b)
            {
                var result = new RaycastResult
                {
                    gameObject = hits[b].collider.gameObject,
                    module = this,
                    distance = hits[b].distance,
                    index = resultAppendList.Count,
                    worldPosition = hits[0].point,
                    worldNormal = hits[0].normal,
                };
                resultAppendList.Add(result);
            }
        }
    }

    /// <summary>
    ///  Perform a Spherecast using the worldSpaceRay in eventData.
    /// </summary>
    /// <param name="eventData"></param>
    /// <param name="resultAppendList"></param>
    /// <param name="radius">Radius of the sphere</param>
    public void Spherecast(PointerEventData eventData, List<RaycastResult> resultAppendList, float radius)
    {
        if (eventCamera == null)
            return;

        if (!eventData.IsVRPointer())
            return;

        Ray ray = eventData.GetRay();
        float dist = eventCamera.farClipPlane - eventCamera.nearClipPlane;
        RaycastHit[] hits = Physics.SphereCastAll(ray, radius, dist, FinalEventMask);

        if (hits.Length > 1)
            System.Array.Sort(hits, (r1, r2) => r1.distance.CompareTo(r2.distance));

        if (hits.Length != 0)
        {
            for (int b = 0, bmax = hits.Length; b < bmax; ++b)
            {
                RaycastResult result = new RaycastResult
                {
                    gameObject = hits[b].collider.gameObject,
                    module = this,
                    distance = hits[b].distance,
                    index = resultAppendList.Count,
                    worldPosition = hits[0].point,
                    worldNormal = hits[0].normal,
                };
                resultAppendList.Add(result);
            }
        }
    }
    /// <summary>
    /// Get screen position of this world position as seen by the event camera of this VRPhysicsRaycaster
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <returns></returns>
    public Vector2 GetScreenPos(Vector3 worldPosition)
    {
        // In future versions of Unity RaycastResult will contain screenPosition so this will not be necessary
        return eventCamera.WorldToScreenPoint(worldPosition);
    }
}

