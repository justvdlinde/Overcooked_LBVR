using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCharacter
{
	// Place this script directly on the OVR rig
	[RequireComponent(typeof(Rigidbody))]
	public class PhysicsCharacterController : MonoBehaviour
	{
		private Rigidbody rigidBody = null;
		[SerializeField] private Transform headAnchor = null;

		public float movementSpeed = 1f;
		public float rotationSpeed = 1f;

		private float slowDownVelocity = 0.75f;
		private float slowDownRotation = 0.75f;

		private float maxPositionChange = 5f;
		private float maxRotationChange = 5000f;

		private Vector3 targetPos = Vector3.zero;
		private Quaternion targetRot = Quaternion.identity;
		public LayerMask groundedLayers = 0;

		private PlayerCharacterPositionDelegate positionDelegate = null;
		public float gravMultiplier = 1f;

		[Header("Movement type values")]
		[SerializeField] private bool useTeleport = false;
		[SerializeField] private bool useSnapturn = false;
		[SerializeField] private float snapTurnAmount = 15f;
		[SerializeField] public Hand preferredHand;
		[SerializeField] private bool moveUsingHandNotHead = false;
		private bool canSnapTurn = true;
		private const float snapTurnDeadzone = 0.7f;
		private const float snapTurnTriggerzone = 0.85f;

		public GameObject fadeBox = null;
		public Renderer fadeRenderer = null;

		public bool isAimingTeleport { get; private set; } = false;

		private float xOnTpStart = 0.0f;
		private bool brokeDeadzone = false;

		private Vector3 fwdOnStart = new Vector3();
		private float deadzoneRange = 0.1f;
		public static float deadzoneRangeXInput = 0.8f;

		private void Awake()
		{
			rigidBody = GetComponent<Rigidbody>();
			
			targetPos = transform.position;
			targetRot = transform.rotation;
		}
		
		private void Start()
		{
			positionDelegate = PlayerCharacterPositionDelegate.Instance;

			//KHS_PhysicsPlayerBlackboard.Instance.AddFixedUpdateListener(new KHS_PhysicsPlayerBlackboard.FixedUpdatePrio(OnFixedUpdate, 1));
		}

		private void Update()
		{
			if(useTeleport)
				UpdateMovementTeleportSnapTurn();
			else if(useSnapturn)
				UpdateMovementSnapTurn();
			else
				UpdateMovementSmooth();
			
			// something here to check teleport inputs
		}

		private void FixedUpdate()
		{
			if(!PhysicsPlayerBlackboard.Instance.isHoldingMovementTool)
				MoveUsingPhysics();
			else
			{
				// left empty intentionally
			}
		}

		public void OnTeleportPlayer(Vector3 point, Vector3 fwd)
		{
			StartCoroutine(DoFadeBox(() => TeleportPlayer(point, fwd)));
		}

		private void TeleportPlayer(Vector3 point, Vector3 fwd)
		{
			Transform rig = transform.GetChild(0);

			transform.position = point;
			transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);

			targetPos = transform.position;
			targetRot = transform.rotation;

			rig.localPosition = Vector3.zero;
			rig.localRotation = Quaternion.identity;

			Vector3 ang = rig.localEulerAngles;
			ang.y = -PhysicsPlayerBlackboard.Instance.headAnchor.localEulerAngles.y;
			rig.localEulerAngles = ang;

			Vector3 pos = rig.localPosition;
			Vector3 headPos = PhysicsPlayerBlackboard.Instance.headAnchor.localPosition;
			pos.x = -headPos.x;
			pos.z = -headPos.z;
			rig.localPosition = pos;
		}

		public void SmoothMovement(Vector2 inputAxis)
		{
			Transform target = null;
			if(moveUsingHandNotHead)
				target = (preferredHand == Hand.Left) ? PhysicsPlayerBlackboard.Instance.leftController : PhysicsPlayerBlackboard.Instance.rightController;
			else
				target = PhysicsPlayerBlackboard.Instance.headAnchor;

			Vector3 fwd = target.forward;
			Vector3 rgt = target.right;

			// remove tilt and yaw from move direction (translate to 2d movement)
			fwd.y = 0f;
			fwd = fwd.normalized;

			rgt.y = 0f;
			rgt = rgt.normalized;

			Vector3 newDirection = ((fwd * inputAxis.y) + (rgt * inputAxis.x)) * Time.deltaTime * movementSpeed;

			targetPos = transform.position + newDirection;
		}

		public void SmoothMovementOnlyFwd(Vector2 inputAxis)
		{
			// movement
			Transform target = null;
			if(moveUsingHandNotHead)
				target = (preferredHand == Hand.Left) ? PhysicsPlayerBlackboard.Instance.leftController : PhysicsPlayerBlackboard.Instance.rightController;
			else
				target = PhysicsPlayerBlackboard.Instance.headAnchor;

			Vector3 fwd = target.forward;


			fwd.y = 0f;
			fwd = fwd.normalized;

			float axisValue = inputAxis.y;
			if(axisValue < 0)
				axisValue = 0f;

			Vector3 newDirection = (fwd * axisValue) * Time.deltaTime * movementSpeed;

			targetPos = transform.position + newDirection;
		}

		public void SnapTurn(Vector2 inputAxis)
		{
			// check if input is greater that snapTurnTriggerzone in its respective axis'
			bool doSnapTurn = (inputAxis.x <= -snapTurnTriggerzone || inputAxis.x >= snapTurnTriggerzone || inputAxis.y <= -snapTurnTriggerzone) && canSnapTurn;

			// reset canSnapTurn if the stick is in resting position, not checking positive inputAxis.y because of movement being handled seperately
			if(inputAxis.x >= -snapTurnDeadzone && inputAxis.x <= snapTurnDeadzone && inputAxis.y >= -snapTurnDeadzone)
				canSnapTurn = true;

			if(canSnapTurn && doSnapTurn)
			{
				if(inputAxis.y <= -snapTurnTriggerzone)
				{
					// do 180
					//transform.Rotate(Vector3.up, 180f);

					StartCoroutine(DoFadeBox(
						() => {
							transform.Rotate(Vector3.up, 180f);
							if(PhysicsPlayerBlackboard.Instance.snapTurnEvent != null)
								PhysicsPlayerBlackboard.Instance.snapTurnEvent.Invoke(180f);
						}
						));

					if(doSnapTurn)
						canSnapTurn = false;
				}
				else if(inputAxis.x <= -snapTurnTriggerzone || inputAxis.x >= snapTurnTriggerzone)
				{
					float val = snapTurnAmount * ((inputAxis.x < 0) ? -1f : 1f);

					//transform.Rotate(Vector3.up, val);

					StartCoroutine(DoFadeBox(
						() => {
							transform.Rotate(Vector3.up, val);
						if(PhysicsPlayerBlackboard.Instance.snapTurnEvent != null)
							PhysicsPlayerBlackboard.Instance.snapTurnEvent.Invoke(val);
								}
						));

					if(doSnapTurn)
						canSnapTurn = false;
				}
			}
		}

		private IEnumerator DoFadeBox(Action callback)
		{
			float timer = 0f;
			float maxTime = 0.25f;

			PhysicsPlayerBlackboard.Instance.isFading = true;

			fadeBox.SetActive(true);
		
			fadeRenderer.material.color *= new Color(1.0f, 1.0f, 1.0f, 0.0f);

			while(true)
			{
				timer += Time.deltaTime;
				yield return null;

				float a = Mathf.Lerp(0.0f, 1.0f, timer / maxTime);

				fadeRenderer.material.color = new Color(0.0f, 0.0f, 0.0f, a);

				if(timer > maxTime)
				{
					fadeRenderer.material.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
					break;
				}
			}

			callback?.Invoke();

			while(true)
			{
				timer += Time.deltaTime;
				yield return null;

				float a = Mathf.Lerp(1.0f, 0.0f, timer / maxTime);

				fadeRenderer.material.color = new Color(0.0f, 0.0f, 0.0f, a);

				if(timer > maxTime)
				{
					fadeBox.SetActive(false);
					break;
				}
			}

			PhysicsPlayerBlackboard.Instance.isFading = false;
		}

		public void SmoothTurn(Vector2 inputAxis)
		{
			transform.Rotate(Vector3.up, inputAxis.x * rotationSpeed * 0.5f);
		}
		

		public void UpdateMovementTeleportSnapTurn()
		{
			Vector2 inputAxis = XRInput.GetThumbStickAxis(preferredHand);

			if(IsGrounded())
			{
				//targetPos = transform.position;

				bool doTeleport = false;

				float yAxis = inputAxis.y;
				if(yAxis < 0f)
					yAxis = 0.0f;

				if(yAxis >= snapTurnDeadzone && isAimingTeleport == false)
				{
					isAimingTeleport = true;
					brokeDeadzone = false;
					xOnTpStart = inputAxis.x;

					fwdOnStart = PhysicsPlayerBlackboard.Instance.GetFollowTarget(preferredHand).forward;
					fwdOnStart.y = 0f;
				}

				float angle = 0.0f;
				Vector3 ang = fwdOnStart;
				if(isAimingTeleport)
				{
					Transform prefHand = PhysicsPlayerBlackboard.Instance.GetFollowTarget(preferredHand);
					PhysicsPlayerBlackboard.Instance.PhysArcPredictor.Predict(prefHand.position, prefHand.forward);

					if(!brokeDeadzone && IsXDeadzoneBroken(inputAxis.x, xOnTpStart))
						brokeDeadzone = true;

					// add head worl eulers to angle by default

					if(brokeDeadzone)
						angle = Mathf.Atan2(inputAxis.x, inputAxis.y) * Mathf.Rad2Deg;
					else
						angle = 0.0f;

					// rotate initial fwd with angle over Vector3.up

					ang = Quaternion.Euler(0, angle, 0) * fwdOnStart;

					if(IsTeleportRestingRange(inputAxis))
					{
						isAimingTeleport = false;
						brokeDeadzone = false;
						if(PhysicsPlayerBlackboard.Instance.HasValidTeleportTarget)
							doTeleport = true;
					}
				}

				// get angle from input
				if(doTeleport)
				{
					fwdOnStart = Vector3.zero;
					PhysicsPlayerBlackboard.Instance.playerTeleportEvent.Invoke(PhysicsPlayerBlackboard.Instance.teleportAnchor.position, ang);
				}

				// check initial deadzone
				// apply teleport on release
			}
			else
			{
				isAimingTeleport = false;
				brokeDeadzone = false;
				SmoothMovementOnlyFwd(inputAxis);
			}


			if(!isAimingTeleport)
				SnapTurn(inputAxis);
		}

		private bool IsTeleportRestingRange(Vector2 axisValue)
		{
			return Vector2.Distance(Vector2.zero, axisValue) < 0.25f;
		}
		private bool IsXDeadzoneBroken(float curX, float initX)
		{
			return curX < initX - (deadzoneRangeXInput) || curX > initX + (deadzoneRangeXInput);
		}

		public void UpdateMovementSnapTurn()
		{
			Vector2 inputAxis = XRInput.GetThumbStickAxis(preferredHand);

			SnapTurn( inputAxis);
			SmoothMovementOnlyFwd(inputAxis);
		}

		public void UpdateMovementSmooth()
		{
			Vector2 inputAxis = XRInput.GetThumbStickAxis(preferredHand);

			Hand otherHand = (preferredHand == Hand.Right) ? Hand.Left : Hand.Right;
			Vector2 rotationAxis = XRInput.GetThumbStickAxis(otherHand);

			SmoothTurn(rotationAxis);
			SmoothMovement(inputAxis);
		}

		private bool IsGrounded()
		{
			return Physics.Raycast(transform.position + (Vector3.up * 0.02f), Vector3.down, 0.1f, groundedLayers);
		}

		float builtUpGravityTime = 0.0f;

		private void MoveUsingPhysics()
		{
			rigidBody.velocity *= 0.995f;

			Vector3 newVelocity = FindNewVelocity();

			if(IsValidVelocity(newVelocity.x))
			{
				float maxChange = maxPositionChange * Time.deltaTime;
				//	rigidBody.velocity = Vector3.MoveTowards(rigidBody.velocity, newVelocity, maxChange);
				//else

				if(!IsGrounded())
				{
					builtUpGravityTime += Time.deltaTime * gravMultiplier;

					Vector3 gravity = Physics.gravity * builtUpGravityTime;

					rigidBody.velocity = Vector3.MoveTowards(rigidBody.velocity, newVelocity, maxChange);

					rigidBody.velocity += gravity * gravMultiplier;
				}
				else
				{
					builtUpGravityTime = 0.0f;
					rigidBody.velocity = Vector3.MoveTowards(rigidBody.velocity, newVelocity, maxChange);
				}

			}
		}

		private Vector3 FindNewVelocity()
		{
			Vector3 diff = targetPos - rigidBody.position;
			return diff / Time.deltaTime;
		}

		private void RotateUsingPhysics()
		{
			rigidBody.angularVelocity *= slowDownRotation;

			Vector3 newAngularVelocity = FindNewAngularVelocity();

			if(IsValidVelocity(newAngularVelocity.x))
			{
				float maxChange = maxRotationChange * Time.deltaTime;
				rigidBody.angularVelocity = Vector3.MoveTowards(rigidBody.angularVelocity, newAngularVelocity, maxChange);
			}
		}

		private Vector3 FindNewAngularVelocity()
		{
			Quaternion diff = targetRot * Quaternion.Inverse(rigidBody.rotation);
			diff.ToAngleAxis(out float angle, out Vector3 rotationAxis);

			if(angle > 180f)
				angle -= 360f;

			return (rotationAxis * angle * Mathf.Deg2Rad) / (Time.deltaTime);
		}

		private bool IsValidVelocity(float x)
		{
			return !float.IsNaN(x) && !float.IsInfinity(x);
		}

		public void FreezePhysics(bool freeze)
		{
			rigidBody.velocity *= 0f;
			rigidBody.angularVelocity *= 0f;
			rigidBody.useGravity = !freeze;
			rigidBody.isKinematic = freeze;
		}
	}
}
