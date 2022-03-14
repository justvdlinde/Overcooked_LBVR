using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PhysicsCharacter;

public class TeleportTrajectoryGraphics : MonoBehaviour
{
	[SerializeField]
	private GameObject viewRoot;
	[SerializeField]
	private Transform targetObject;
	[SerializeField]
	private Transform targetGraphic;
	[SerializeField]
	private LineRenderer trajectory;

	private Vector3 targetRotation;

	private bool isActive;

	[SerializeField] private Gradient validTargetColor = new Gradient();
	[SerializeField] private Gradient invalidTargetColor = new Gradient();

	private float yAxisOnStartTP = 0.0f;
	private bool yAxisRegistered = false;
	


	private void Awake()
	{
		SetViewActive(false);
	}

	private void SetViewActive(bool active)
	{
		isActive = active;
		viewRoot.SetActive(active);
	}

	private Vector3 fwdOnStart = new Vector3();
	private float xOnStart = 0.0f;
	private bool brokeDeadzone = false;

	private void LateUpdate()
	{
		// need some local check
		//if(!playerPawn.IsLocal) return;

		// register head orientation Y angle on first frame of active

		bool hasCollisionTarget = PhysicsPlayerBlackboard.Instance.HasValidTeleportTarget;
		// teleporter graphics active
		bool isActiveView = PhysicsPlayerBlackboard.Instance.IsAimingTeleport;

		Vector2 axis = XRInput.GetThumbStickAxis(PhysicsPlayerBlackboard.Instance.PreferredHand);

		if(isActiveView && !yAxisRegistered)
		{
			yAxisRegistered = true;
			fwdOnStart = PhysicsPlayerBlackboard.Instance.headAnchor.forward;
			fwdOnStart.y = 0f;
			xOnStart = axis.x;
			brokeDeadzone = false;
		}
		if(!isActiveView)
		{
			fwdOnStart = Vector3.forward;
			yAxisRegistered = false;
		}

		targetGraphic.gameObject.SetActive(false);
		if(hasCollisionTarget && isActiveView)
		{
			targetRotation = targetGraphic.localRotation.eulerAngles;
			targetGraphic.gameObject.SetActive(true);
			trajectory.colorGradient = validTargetColor;
		}
		else if(isActiveView)
		{
			trajectory.colorGradient = invalidTargetColor;
			targetGraphic.gameObject.SetActive(false);
		}

		Vector3 cursorFwd = fwdOnStart;

		SetViewActive(isActiveView);

		trajectory.positionCount = PhysicsPlayerBlackboard.Instance.PhysArcPredictor.PredictedPositions.Length;
		trajectory.SetPositions(PhysicsPlayerBlackboard.Instance.PhysArcPredictor.PredictedPositions);

		viewRoot.transform.position = PhysicsPlayerBlackboard.Instance.PhysArcPredictor.RaycastHit.point;

		if(!brokeDeadzone && IsXDeadzoneBroken(axis.x, xOnStart))
			brokeDeadzone = true;

		float angle = 0.0f;
		if(brokeDeadzone)
			angle = Mathf.Atan2(axis.x, axis.y) * Mathf.Rad2Deg;
		else
			angle = 0.0f;

		cursorFwd = Quaternion.Euler(0, angle, 0) * fwdOnStart;

		targetObject.localRotation = Quaternion.Euler(0, PhysicsPlayerBlackboard.Instance.headAnchor.rotation.eulerAngles.y, 0);
		targetGraphic.forward = cursorFwd;
		
		//if(isActive) SetViewActive(false);
	}

	private bool IsXDeadzoneBroken(float curX, float initX)
	{
		return curX < initX - (PhysicsCharacterController.deadzoneRangeXInput) || curX > initX + (PhysicsCharacterController.deadzoneRangeXInput);
	}
}
