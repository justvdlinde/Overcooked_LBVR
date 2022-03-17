using UnityEngine;
using UnityEngine.InputSystem;

public class OperatorUIController : MonoBehaviour
{
    protected OperatorControls input;

    [SerializeField] private OperatorUIMenu menu = null;

    public void InjectDependencies(OperatorControls input)
    {
        this.input = input;
    }

    private void Awake()
    {
        input.OperatorControlsMap.OpenMenu.performed += ToggleMenuAction;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        input.OperatorControlsMap.OpenMenu.performed -= ToggleMenuAction;
    }

    private void ToggleMenuAction(InputAction.CallbackContext context)
    {
        if (menu.IsOpen)
            menu.Close();
        else
            menu.Open();
    }
}
