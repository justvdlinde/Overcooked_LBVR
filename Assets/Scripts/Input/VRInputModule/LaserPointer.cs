using UnityEngine;

public class LaserPointer : VRCursor
{
    public enum LaserBeamBehavior
    {
        On,        // laser beam always on
        Off,        // laser beam always off
        OnWhenHitTarget,  // laser beam only activates when hit valid target
    }

    public GameObject cursorVisual;
    public float maxLength = 10.0f;
    public float cursorDepthOffset = 0.01f;

    public LaserBeamBehavior LaserBehavior
    {
        set
        {
            laserBeamBehavior = value;
            if (LaserBehavior == LaserBeamBehavior.Off || LaserBehavior == LaserBeamBehavior.OnWhenHitTarget)
            {
                lineRenderer.enabled = false;
            }
            else
            {
                lineRenderer.enabled = true;
            }
        }
        get
        {
            return laserBeamBehavior;
        }
    }
    [SerializeField] private LaserBeamBehavior laserBeamBehavior = LaserBeamBehavior.On;

    private Vector3 startPoint;
    private Vector3 forward;
    private Vector3 endPoint;
    private bool isHitTarget;
    private Vector3 normal;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        if (cursorVisual) cursorVisual.SetActive(false);
    }

    void OnDisable()
    {
        if (cursorVisual)
            cursorVisual.SetActive(false);
    }

    public override void SetCursorStartDest(Vector3 start, Vector3 dest, Vector3 normal)
    {
        startPoint = start;
        endPoint = dest;
        isHitTarget = true;
        this.normal = normal;
    }

    public override void SetCursorRay(Transform t)
    {
        startPoint = t.position;
        forward = t.forward;
        isHitTarget = false;
    }

    private void LateUpdate()
    {
        lineRenderer.SetPosition(0, startPoint);
        if (isHitTarget)
        {
            lineRenderer.SetPosition(1, endPoint);
            UpdateLaserBeam(startPoint, endPoint);
            if (cursorVisual)
            {
                cursorVisual.transform.position = endPoint + (startPoint - endPoint) * cursorDepthOffset;
                cursorVisual.SetActive(true);
                cursorVisual.transform.rotation = Quaternion.Euler(normal);
            }
        }
        else
        {
            UpdateLaserBeam(startPoint, startPoint + maxLength * forward);
            lineRenderer.SetPosition(1, startPoint + maxLength * forward);
            if (cursorVisual) cursorVisual.SetActive(false);
        }
    }

    private void UpdateLaserBeam(Vector3 start, Vector3 end)
    {
        if (LaserBehavior == LaserBeamBehavior.Off)
        {
            return;
        }
        else if (LaserBehavior == LaserBeamBehavior.On)
        {
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }
        else if (LaserBehavior == LaserBeamBehavior.OnWhenHitTarget)
        {
            if (isHitTarget)
            {
                if (!lineRenderer.enabled)
                {
                    lineRenderer.enabled = true;
                    lineRenderer.SetPosition(0, start);
                    lineRenderer.SetPosition(1, end);
                }
            }
            else
            {
                if (lineRenderer.enabled)
                {
                    lineRenderer.enabled = false;
                }
            }
        }
    }
}
