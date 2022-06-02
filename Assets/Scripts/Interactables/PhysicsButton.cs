using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utils.Core.Attributes;

public class PhysicsButton : MonoBehaviour
{
	private bool IsUsable = true;

	[Header("Button values")]
	[SerializeField] private float clickThreshold = 0.75f;
	[SerializeField] private UnityEvent onPress = null;
	[SerializeField] private UnityEvent onRelease = null;
	[Header("Presets")]
	[SerializeField] private Rigidbody rb = null;
	[SerializeField] private Transform upperLimit = null;
	[SerializeField] private Transform lowerLimit = null;
	[SerializeField] private Renderer[] connectedRenderers = null;
	[SerializeField] private Material availableMat = null;
	[SerializeField] private Material unavailableMat = null;
	[SerializeField] private Material hoverMat = null;
	[SerializeField] private bool useOwnMatswitch = true;
	[SerializeField] private AudioSource audioSource = null;

	private bool isPressed = false;
	private bool previousState = false;
	private float lerpVal;

	public Action PressEvent { get; set; }
	public Action ReleaseEvent { get; set; }

	private void Update()
	{
		Vector3 rbPos = rb.transform.localPosition;
		Vector3 rbVelocity = transform.InverseTransformDirection(rb.velocity);
		rbPos.x = 0f;
		rbPos.z = 0f;
		rbVelocity.z = 0f;
		rbVelocity.x = 0f;

		rb.transform.localPosition = rbPos;

		lerpVal = Mathf.Clamp01(InverseLerp(upperLimit.localPosition, lowerLimit.localPosition, rbPos));
		rbPos = Vector3.Lerp(upperLimit.localPosition, lowerLimit.localPosition, lerpVal);
		rb.transform.localPosition = rbPos;
		rb.velocity = transform.TransformDirection(rbVelocity);

		previousState = isPressed;
		if (lerpVal >= clickThreshold)
		{
			if (!isPressed)
			{
				OnClick();
				isPressed = true;
				if (useOwnMatswitch)
				{
					foreach (Renderer ren in connectedRenderers)
					{
						ren.material = (isPressed) ? hoverMat : availableMat;
					}
				}
			}
		}
		else
		{
			if (isPressed)
			{
				OnRelease();
				isPressed = false;
				if (useOwnMatswitch)
				{
					foreach (Renderer ren in connectedRenderers)
					{
						ren.material = (isPressed) ? hoverMat : availableMat;
					}
				}
			}
		}

		//connectedRenderer.materials[1] = (isPressed) ? hoverMat : availableMat;
	}

	public void SetState(bool usable)
	{
		IsUsable = usable;
	}

	[Button]
	public void OnClick()
	{
		if (!IsUsable)
			return;
		if (onPress != null)
		{
			StartCoroutine(DoButtonDelay());
			onPress.Invoke();
		}

		PressEvent?.Invoke();

		audioSource.Play();
	}

	public void ForceButtonClick()
	{
		Vector3 rbPos = rb.transform.localPosition;
		Vector3 rbVelocity = transform.InverseTransformDirection(rb.velocity);
		rbPos.x = 0f;
		rbPos.z = 0f;
		rbVelocity.z = 0f;
		rbVelocity.x = 0f;

		rb.transform.localPosition = rbPos;

		lerpVal = 1f;
		rbPos = Vector3.Lerp(upperLimit.localPosition, lowerLimit.localPosition, lerpVal);
		rb.transform.localPosition = rbPos;
		rb.velocity = transform.TransformDirection(rbVelocity);

		OnClick();
	}

	public void OnRelease()
	{
		if (!IsUsable)
			return;
		if (onRelease != null)
			onRelease.Invoke();
	}

	public float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
	{
		Vector3 AB = b - a;
		Vector3 AV = value - a;
		return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
	}

	public void SetButtonState(bool state)
	{
		IsUsable = state;

		if (useOwnMatswitch)
		{
			if (IsUsable == false)
			{
				foreach (Renderer ren in connectedRenderers)
				{
					ren.material = unavailableMat;
				}
			}
			else
			{
				foreach (Renderer ren in connectedRenderers)
				{
					ren.material = availableMat;
				}
			}
		}
	}

	private IEnumerator DoButtonDelay()
	{
		SetButtonState(false);
		yield return new WaitForSeconds(0.5f);
		SetButtonState(true);
	}
}
