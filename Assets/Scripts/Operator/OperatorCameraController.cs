using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Utils.Core.Extensions;

public class OperatorCameraController : MonoBehaviour
{
    [Tooltip("The active camera during scene load and in case there are no camera's in the level")]
    [SerializeField] protected SpectatorCamera defaultCamera = null;

    protected SpectatorCamera CurrentActiveCamera;
    protected int activeCameraIndex = -1;

    protected OperatorControls input;

    public void InjectDependencies(OperatorControls input)
    {
        this.input = input;
    }

    private void Awake()
    {
        input.OperatorControlsMap.ChangeCamera.performed += ChangeCamera;
        SceneManager.activeSceneChanged += OnSceneChangedEvent;
    }

    private void Start()
    {
        SetActiveCamera(defaultCamera);
    }

    private void OnDestroy()
    {
        input.OperatorControlsMap.ChangeCamera.performed -= ChangeCamera;
        SceneManager.activeSceneChanged -= OnSceneChangedEvent;
    }

    private void ChangeCamera(InputAction.CallbackContext context)
    {
        float direction = context.ReadValue<float>();
        direction = Mathf.Clamp(direction, -1, 1);
        activeCameraIndex += (int)direction;

        if (SpectatorCamera.LevelCameras.Count == 0)
        {
            activeCameraIndex = -1;
            SetActiveCamera(defaultCamera);
        }
        else
        {
            activeCameraIndex = SpectatorCamera.LevelCameras.GetLoopingIndex(activeCameraIndex);
            SetActiveCamera(SpectatorCamera.LevelCameras[activeCameraIndex]);
        }
    }

    protected void SetActiveCamera(SpectatorCamera newCamera)
    {
        if (CurrentActiveCamera != null)
            CurrentActiveCamera.Deactivate();

        newCamera.Activate();
        CurrentActiveCamera = newCamera;
    }

    private void OnSceneChangedEvent(Scene from, Scene to)
    {
        activeCameraIndex = -1;
        if (SpectatorCamera.LevelCameras.Count > 1)
        {
            activeCameraIndex = 1;
            SetActiveCamera(SpectatorCamera.LevelCameras[activeCameraIndex]);
        }
        else
        {
            SetActiveCamera(defaultCamera);
        }
    }
}
