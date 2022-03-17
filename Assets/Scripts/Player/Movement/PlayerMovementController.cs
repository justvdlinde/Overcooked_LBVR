using UnityEngine;
using UnityEngine.InputSystem;
using Utils.Core.Events;

public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 1;
    [SerializeField] private float rotationSpeed = 1;
    [SerializeField] private Transform playerTransform = null;
    [SerializeField] private new Camera camera = null;

    private GlobalEventDispatcher globalEventDispatcher;
    private PlayerControls input;

    public void InjectDependencies(PlayerControls input, GlobalEventDispatcher globalEventDispatcher)
    {
        this.globalEventDispatcher = globalEventDispatcher;
        this.input = input;
    }

    private void Awake()
    {
        globalEventDispatcher.Subscribe<SceneLoadedEvent>(OnSceneLoadedEvent);
    }

    private void OnDestroy()
    {
        input.Disable();
        globalEventDispatcher.Unsubscribe<SceneLoadedEvent>(OnSceneLoadedEvent);
    }

    private void OnSceneLoadedEvent(SceneLoadedEvent obj)
    {
        ResetYPosition();
    }

    private void ResetYPosition()
    {
        playerTransform.position = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update()
    {
        Movement();
        Rotation();
    }
#endif

    private void Movement()
    {
        Vector2 translation = input.RightHand.Primary2DAxis.ReadValue<Vector2>() * movementSpeed * Time.deltaTime;
        Vector3 targetDirection = new Vector3(translation.x, 0f, translation.y);
        targetDirection = camera.transform.TransformDirection(targetDirection);
        targetDirection.y = 0.0f;

        playerTransform.position += targetDirection;
    }

    private void Rotation()
    {
        Vector2 rotation = input.LeftHand.Primary2DAxis.ReadValue<Vector2>() * rotationSpeed * Time.deltaTime;
        playerTransform.Rotate(0, rotation.x, 0, Space.World);
    }
}
