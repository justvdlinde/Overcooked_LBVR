using TMPro;
using UnityEngine;

public class PlayerMarker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textLabel = null;
	[SerializeField] private LayerMask lineOfSightLayerMask = default;

	private IPlayer owner;

	public void InjectDependencies(IPlayer owner)
	{
		SetOwner(owner);
	}

	public virtual void SetOwner(IPlayer player)
    {
		if(owner != null)
			owner.NameChangeEvent -= OnPlayerNameChangedEvent;

		owner = player;
		owner.NameChangeEvent += OnPlayerNameChangedEvent;
		textLabel.text = player.Name;
    }

	protected virtual void OnDestroy()
    {
		owner.NameChangeEvent -= OnPlayerNameChangedEvent;
	}

	private void OnPlayerNameChangedEvent(string oldName, string newName)
    {
		textLabel.text = newName;
    }

	private void Update()
	{
		LookAt(Camera.main.transform);
		//textLabel.gameObject.SetActive(UIHasLineOfSightOnTarget(Camera.main.transform));
	}

	public bool HasLineOfSightOnTarget(Transform target)
	{
		Vector3 origin = textLabel.transform.position;
		Vector3 targetPos = target.position;

		Debug.DrawRay(origin, (targetPos - origin) * 999, Color.red, 0f);
		if (Physics.Raycast(origin, targetPos - origin, 999, lineOfSightLayerMask))
        {
			// TODO:
			// if hits player, return true, else false
        }
		return false;
	}

	private void LookAt(Transform target)
	{
		Vector3 lookPos = transform.position - target.position;
		lookPos.y = 0;

		// This stops the annoying 'Look rotation viewing vector is zero' error
		if (lookPos != Vector3.zero)
			transform.rotation = Quaternion.LookRotation(lookPos); // Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping); // smooth rotation
	}
}
