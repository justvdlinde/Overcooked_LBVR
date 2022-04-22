using UnityEngine;

public class SauceRotationHandler : MonoBehaviour
{
    [SerializeField] private Vector3 desiredRotation = new Vector3();

	private void Awake()
	{
		transform.parent = null;
	}

	private void Update()
	{
		transform.eulerAngles = desiredRotation;
	}
}
