using UnityEngine;

namespace LW_IK
{
	public class LW_TorsoIK : MonoBehaviour
	{
        [Header("Mandatory bones")]
        [SerializeField] private Transform head = null;
		[SerializeField] private Transform torso = null;
        [SerializeField] private Transform baseNeck = null;
        [SerializeField] private Transform leftHand = null;
        [SerializeField] private Transform rightHand = null;

        [Header("Optional bones")]
		[SerializeField] private Transform waist = null;
        [SerializeField] private Transform leftShoulder = null;
        [SerializeField] private Transform rightShoulder = null;

        [Header("Fields")]
        [SerializeField] private float headNeckDistance = 1;
        [SerializeField] private float neckTorsoDistance = 1;

        private Transform leftHandFollowObject = null;
		private Transform rightHandFollowObject = null;
		private Transform headFollowObject = null;

		private Vector3 avgHandPos;
		private Quaternion frontRotation;

		private float playerHeight = 1.80f;
		private float playerWidth = 0.39f;
		private float headSize = 0.225f;

		private Vector3 frontDirection;
		private Vector3 frontDirectionWaist;
		private Vector3 frontDirectionHands;

		private float neckDeadzone = 35f;
		private float avgHandsDot = 0f;

		private float prevAmt = 0f;
		[SerializeField] private float torsoFollowSpeed = 1f;
		[SerializeField] private float torsoFollowMaxRange = 0.2f;
		[SerializeField] private float torsoRotationSpeed = 6f;
		[SerializeField] private Transform torsoGraphicsForSwaying = null;
		[SerializeField] private Transform hipGraphicsForSwaying = null;
		private Vector3 localTorsoOffset = new Vector3();
		public bool wantToSway = true;

		private void Start()
		{
			localTorsoOffset = torso.localPosition;
			// Davinci proportions
			headSize = playerHeight * 0.125f;

			baseNeck.position = head.position - head.up * headNeckDistance;

			Vector3 neckForward = head.forward;
			neckForward.y = 0;
			baseNeck.forward = neckForward;

			if(leftShoulder != null)
			{
				leftShoulder.position = baseNeck.position - baseNeck.right * (playerWidth * 0.5f);
				leftShoulder.forward = neckForward;
			}

			if(rightShoulder != null)
			{
				rightShoulder.position = baseNeck.position + baseNeck.right * (playerWidth * 0.5f);
				rightShoulder.forward = neckForward;
			}

			if(waist != null)
			{
				waist.position = baseNeck.position + Vector3.down * (headSize * 3);
				waist.forward = baseNeck.forward;
			}

			frontDirection = head.forward;
			frontDirection.y = 0f;

			frontRotation.eulerAngles = frontDirection;

			torso.position = baseNeck.position - Vector3.up * neckTorsoDistance;
		}

		private void Update()
		{
			baseNeck.position = head.position - head.up * headNeckDistance;

			Vector3 neckForward = head.forward;
			neckForward.y = 0;
			baseNeck.forward = neckForward;

			avgHandPos = VectorMidpoint(leftHand.position + neckForward, rightHand.position + neckForward);

			avgHandsDot = Vector3.Dot(neckForward.normalized, (RemoveYOfVector3(avgHandPos) - RemoveYOfVector3(baseNeck.position)).normalized);

			if(avgHandsDot >= -0.1f && Vector3.Distance(avgHandPos, baseNeck.position + Vector3.down * (headSize * 3)) >= 0.15f)
			{
				// hands in front of avatar
				frontDirectionHands = RemoveYOfVector3(avgHandPos) - RemoveYOfVector3(baseNeck.position);
			}
			else
			{
				// idle
				frontDirectionHands = RemoveYOfVector3(head.forward);
			}

			if(leftShoulder != null)
			{
				leftShoulder.position = baseNeck.position - baseNeck.right * (playerWidth * 0.5f);
			}
			if(rightShoulder != null)
			{
				rightShoulder.position = baseNeck.position + baseNeck.right * (playerWidth * 0.5f);
			}
			if(waist != null)
			{
				waist.position = baseNeck.position + Vector3.down * (headSize * 3);
				waist.rotation = Quaternion.Lerp(head.rotation, Quaternion.Euler(frontDirection), 0.5f);
			}

			float diffHeadforwardHandForwardLeft = (Mathf.Abs(1f - Vector3.Dot(neckForward, leftHand.forward)));
			float diffHeadforwardHandForwardRight = (Mathf.Abs(1f - Vector3.Dot(neckForward, rightHand.forward)));

			float angle = Vector3.Angle(frontDirection, neckForward);
			bool isLookingRight = Vector3.Dot(frontDirection, baseNeck.right) >= 0;


			if(angle > neckDeadzone)
			{
				float mag = ((isLookingRight) ? -1 : 1) * (angle - neckDeadzone);
				Vector3 targetRotation = Quaternion.AngleAxis(mag, Vector3.up) * frontDirection;

				frontRotation = Quaternion.FromToRotation(frontDirection, targetRotation) * frontRotation;
				frontDirection = targetRotation;
			}
			else
			{
				Vector3 targetRotation = neckForward;

				frontRotation = Quaternion.FromToRotation(frontDirection, targetRotation) * frontRotation;
				frontDirection = targetRotation;
			}

			float amt = Mathf.Clamp01(/*0.2f + */(avgHandsDot));
			float prevA = prevAmt;
			prevAmt = amt;

			float newAmt = Mathf.Lerp(amt, prevA, 0.5f);

			Quaternion ro = Quaternion.FromToRotation(RemoveYOfVector3(frontRotation.eulerAngles), RemoveYOfVector3(frontDirectionHands));
			Quaternion newRot = Quaternion.RotateTowards(torso.rotation, ro, 7.5f);

			torso.rotation = newRot;

			Vector3 wantedTorsoPosition = baseNeck.position - Vector3.up * neckTorsoDistance;

			float torsoSpeedMultiplier = Mathf.Lerp(1f, 10f,Mathf.InverseLerp(0, torsoFollowMaxRange, Vector3.Distance(torso.position, wantedTorsoPosition)));

			torso.position = Vector3.Lerp(torso.position, wantedTorsoPosition, Time.deltaTime * torsoFollowSpeed * torsoSpeedMultiplier);

			float wantedAngleZ = AngleBetweenVector2(Vector3ToVector2YZ(head.position - torsoGraphicsForSwaying.position), Vector3.up);
			float wantedAngle = AngleBetweenVector2((head.position - torsoGraphicsForSwaying.position), Vector3.up);
			wantedAngle -= 90f;
			wantedAngleZ -= 90f;
			wantedAngle *= torsoRotationSpeed;
			wantedAngle = Mathf.Clamp(wantedAngle, -15f, 15f);

			wantedAngle -= wantedAngleZ;

			wantedAngleZ *= -1;
			wantedAngle *= -1;

			if(wantToSway)
			{

				Vector3 desiredEulers = new Vector3(wantedAngle, torsoGraphicsForSwaying.localEulerAngles.y, wantedAngleZ);

				torsoGraphicsForSwaying.localRotation = Quaternion.Slerp(torsoGraphicsForSwaying.localRotation, Quaternion.Euler(desiredEulers), Time.deltaTime * torsoRotationSpeed);

				hipGraphicsForSwaying.localRotation = Quaternion.Slerp(torsoGraphicsForSwaying.localRotation, Quaternion.Euler(desiredEulers * Mathf.Lerp(2f, 4f, Mathf.InverseLerp(0, torsoFollowMaxRange, Vector3.Distance(torso.position, wantedTorsoPosition)))), Time.deltaTime * torsoRotationSpeed);
			}

			//torsoGraphicsForSwaying.localEulerAngles = desiredEulers;


			//hipGraphicsForSwaying.localEulerAngles = desiredEulers * Mathf.Lerp(2f, 4f, Mathf.InverseLerp(0, torsoFollowMaxRange, Vector3.Distance(torso.position, wantedTorsoPosition)));

			//torsoGraphicsForSwaying.LookAt(head.position, Vector3.up);
			//torsoGraphicsForSwaying.Rotate(-90,0,0);

			if(leftShoulder != null)
			{
				leftShoulder.rotation = Quaternion.FromToRotation(Vector3.up, leftShoulder.position - leftHand.position);
			}
			if(rightShoulder != null)
			{
				rightShoulder.rotation = Quaternion.FromToRotation(Vector3.up, rightShoulder.position - rightHand.position);
			}

			if(leftHandFollowObject != null)
			{
				leftHand.position = leftHandFollowObject.position;
				leftHand.forward = leftHandFollowObject.forward;
			}

			if(rightHandFollowObject != null)
			{
				rightHand.position = rightHandFollowObject.position;
				rightHand.forward = rightHandFollowObject.forward;
			}

			if(headFollowObject != null)
			{
				head.position = headFollowObject.position;
				head.forward = headFollowObject.forward;
			}
		}

		/// <summary>
		/// Set in meters. 1.8f means 1.80m
		/// </summary>
		/// <param name="height"></param>
		public void SetPlayerHeight(float height)
		{
			playerHeight = height;
			headSize = playerHeight * 0.125f;
		}

		/// <summary>
		/// Set object which the hand object will follow. Make sure the forward is set to the direction the thumb points towards.
		/// </summary>
		/// <param name="hand"></param>
		public void SetLeftHand(Transform hand)
		{
			leftHandFollowObject = hand;
		}

		/// <summary>
		/// Set object which the hand object will follow. Make sure the forward is set to the direction the thumb points towards.
		/// </summary>
		/// <param name="hand"></param>
		public void SetRightHand(Transform hand)
		{
			rightHandFollowObject = hand;
		}

		/// <summary>
		/// Set object which the head object will follow. Make sure the forward is set to the direction the camera faces.
		/// </summary>
		/// <param name="hand"></param>
		public void SetHead(Transform headObj)
		{
			headFollowObject = headObj;
		}

		private Vector3 VectorMidpoint(Vector3 a, Vector3 b)
		{
			return new Vector3((a.x + b.x) * 0.5f, (a.y + b.y) * 0.5f, (a.z + b.z) * 0.5f);
		}

		private Vector3 RemoveYOfVector3(Vector3 v3)
		{
			Vector3 v = v3;
			v.y = 0;
			return v;
		}

		private Vector2 Vector3ToVector2YZ(Vector3 v3)
		{
			Vector2 v = new Vector2();
			v.x = v3.z;
			v.y = v3.y;
			return v;
		}

		private float AngleBetweenVector2(Vector2 vec1, Vector2 vec2)
		{
			Vector2 diference = vec2 - vec1;
			float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;
			return Vector2.Angle(Vector2.right, diference) * sign;
		}
	}
}
