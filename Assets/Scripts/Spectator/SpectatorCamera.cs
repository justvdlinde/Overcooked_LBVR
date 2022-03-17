using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SpectatorCamera : MonoBehaviour
{
    public static List<SpectatorCamera> LevelCameras = new List<SpectatorCamera>();

    public bool IsActive { get; protected set; }
    public Camera Camera => camera;

    [SerializeField] private new Camera camera = null;

    private string originalTag;

    protected virtual void Awake()
    {
        LevelCameras.Add(this);
        originalTag = gameObject.tag;
        Deactivate();
    }

    protected virtual void OnDestroy()
    {
        if (LevelCameras.Contains(this))
            LevelCameras.Remove(this);
    }

    public virtual void Activate()
    {
        originalTag = gameObject.tag;
        tag = Tags.MAIN_CAMERA;
        gameObject.SetActive(true);
    }

    public virtual void Deactivate()
    {
        gameObject.tag = originalTag;
        gameObject.SetActive(false);
    }
}
