using UnityEngine;
using UnityEngine.InputSystem;

public class PistolController : MonoBehaviour
{
	[SerializeField] private Pistol pistol = null;
	[SerializeField] private Hand hand = Hand.Right;
    [SerializeField] private float hapticsDuration = 0.2f;
    [SerializeField] private float hapticsStrength = 1f;

    private PlayerControls input;

	public void InjectDependencies(PlayerControls input)
    {
		SetInputControls(input);
    }

    public void SetInputControls(PlayerControls controls)
    {
        input = controls;
    }

    private void OnEnable()
    {
        if(hand == Hand.Right)
            input.RightHand.TriggerPress.performed += OnTriggerPress;
        else
            input.LeftHand.TriggerPress.performed += OnTriggerPress;
    }

    private void OnDisable()
    {
        input.RightHand.TriggerPress.performed -= OnTriggerPress;
        input.LeftHand.TriggerPress.performed -= OnTriggerPress;
    }

    private void OnTriggerPress(InputAction.CallbackContext context)
    {
        if (pistol.CanShoot)
        {
            pistol.Shoot();
            XRInput.PlayHaptics(hand, hapticsStrength, hapticsDuration);
        }
    }

	public void Update()
	{
		pistol.SetTriggerValue(hand == Hand.Left ? input.LeftHand.Trigger.ReadValue<float>() : input.RightHand.Trigger.ReadValue<float>());
    }
}
