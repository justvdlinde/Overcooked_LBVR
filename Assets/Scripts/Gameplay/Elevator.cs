using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Elevator : MonoBehaviourPunCallbacks, IResettable
{
	public enum State
	{
		Up,
		Down
	}

	public State CurrentState { get; protected set; }
	public bool IsMoving { get; protected set; }

	[SerializeField] private float speed = 1f;
	[Tooltip("Initial delay in seconds before moving the bucket, can be used to close/open doors")]
	[SerializeField] private float moveDelay = 1f;

	[SerializeField] private State startingState = State.Down;
	[SerializeField] private PhysicsButton button = null;
	[SerializeField] private Transform platform = null;
	[SerializeField] private Transform upPosition = null;
	[SerializeField] private Transform downPosition = null;

	protected Coroutine activeCoroutine;

    private void Start()
	{
		MoveImmediate(startingState);

		if(button != null)
			button.SetState(startingState == State.Down);
	}

	public override void OnJoinedRoom()
	{
		if (!PhotonNetwork.IsMasterClient)
			photonView.RPC(nameof(RequestStateRPC), RpcTarget.MasterClient);
	}

	public override void OnEnable()
    {
        base.OnEnable();
		button.PressEvent += Move;
    }

    public override void OnDisable()
    {
        base.OnDisable();
		button.PressEvent -= Move;
	}

    public void Reset()
	{
		if (Application.isPlaying)
		{
			if (PhotonNetwork.IsMasterClient)
			{
				if (CurrentState != startingState)
					Move(startingState);
			}
		}
	}

	[PunRPC]
	private void RequestStateRPC(PhotonMessageInfo info)
	{
		if (PhotonNetwork.IsMasterClient)
			photonView.RPC(nameof(SetStateRPC), info.Sender, (int)CurrentState);
	}

	[PunRPC]
	private void SetStateRPC(int state)
	{
		MoveImmediate((State)state);
	}

	public void MoveImmediate(State state)
	{
		CurrentState = state;
		IsMoving = false;

		if (activeCoroutine != null)
		{
			StopCoroutine(activeCoroutine);
			activeCoroutine = null;
		}
		platform.transform.position = (state == State.Down) ? downPosition.position : upPosition.position;
	}

	public void Move()
	{
		Move(CurrentState == State.Down ? State.Up : State.Down);
	}

	public void MoveUp()
    {
		Move(State.Up);
    }

	public void MoveDown()
	{
		Move(State.Down);
	}

	public void Move(State newState)
	{
		if (IsMoving || CurrentState == newState)
			return;

		//photonView.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
		//PhotonNetwork.RemoveRPCs(photonView);
		photonView.RPC(nameof(MoveRPC), RpcTarget.All, (int)newState);
		//photonView.TransferOwnership(-1);
	}

	[PunRPC]
	public void MoveRPC(int newStateId, PhotonMessageInfo info)
	{
		State newState = (State)newStateId;
		if (IsMoving || CurrentState == newState)
			return;

		Vector3 endPoint = newState == State.Down ? downPosition.position : upPosition.position;
		activeCoroutine = StartCoroutine(MoveCoroutine(newState, endPoint));
	}

	private IEnumerator MoveCoroutine(State newState, Vector3 targetPosition)
	{
		IsMoving = true;
		Vector3 startPos = platform.transform.position;

		float elapsedTime = 0;
		float distance = Vector3.Distance(platform.transform.position, targetPosition);
		float duration = distance / speed;

		yield return new WaitForSeconds(moveDelay);

		while (elapsedTime < duration)
		{
			platform.position = Vector3.Lerp(startPos, targetPosition, (elapsedTime / duration));
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		CurrentState = newState;
		yield return new WaitForSeconds(moveDelay);
		IsMoving = false;
	}
}
