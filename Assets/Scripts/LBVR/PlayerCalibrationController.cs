using UnityEngine;
using Utils.Core.Attributes;

public class PlayerCalibrationController : MonoBehaviour
{
    public float CurrentWaitTime { get; private set; }
    public bool IsCalibrated { get; private set; }

    [SerializeField] private float buttonHoldDuration = 2f;
    [SerializeField] private Transform head = null;
    [SerializeField] private Transform leftHand = null;
    [SerializeField] private Transform rightHand = null;
    [SerializeField] private Transform playerPawn = null;
    [SerializeField] private Transform pawnRoot = null;

    private bool isHoldingB = false;
    private bool isHoldingY = false;
    private bool canCalibrate = true;

    private void Update()
    {
        isHoldingB = XRInput.GetSecondaryButtonPressed(Hand.Right);
        isHoldingY = XRInput.GetSecondaryButtonPressed(Hand.Left);


        if (canCalibrate && isHoldingB && isHoldingY)
        {
            CurrentWaitTime += Time.deltaTime;
            if (CurrentWaitTime > buttonHoldDuration)
            {
                Debug.Log("calibrate");
                Calibrate();
                canCalibrate = false;
            }
        }
        else
        {
            CurrentWaitTime = 0;
            canCalibrate = true;
        }
    }

    [Button]
    private void Calibrate()
    {
        XRInput.PlayHaptics(Hand.Right, 1, 1);
        XRInput.PlayHaptics(Hand.Left, 1, 1);

        pawnRoot.position = new Vector3(0, pawnRoot.position.y, 0);
        pawnRoot.rotation = Quaternion.identity;

        float headRotation = head.transform.eulerAngles.y;


        playerPawn.position = Vector3.zero;

        Vector3 centerOfPlayerPos = head.position - (head.forward.normalized * 0.09f);
        centerOfPlayerPos.y = 0f;

        // TODO: clean up
        //playerPawn.GetChild(0).position = playerPawn.GetChild(0).position - centerOfPlayerPos;
        playerPawn.position = playerPawn.position - centerOfPlayerPos;

        //Vector3 headPos = head.transform.position;
        //float headY = headPos.y;
        //headPos *= -1;
        //headPos.y = headY;
        //playerPawn.transform.localPosition = headPos;

        //Vector3 rot = playerPawn.transform.eulerAngles;
        //rot.y = -headRotation;
        //playerPawn.transform.eulerAngles = rot;
        
        


        if (CalibrationCursor.Instance != null)
        {
            playerPawn.position = CalibrationCursor.Instance.GetCalibrationCursorPosition() - centerOfPlayerPos;
        }
        Vector3 newForward = (leftHand.position + rightHand.position) * 0.5f;
        pawnRoot.rotation = GetNewRotation(newForward);

        IsCalibrated = true;
    }

	private Quaternion GetNewRotation(Vector3 playerToHand)
    {
        Quaternion newRot = new Quaternion();

        Vector3 centerOfPlayerPos = head.position - (head.forward.normalized * 0.09f);
        centerOfPlayerPos.y = 0f;

        Vector3 pos0 = Vector3.forward - centerOfPlayerPos;
        //Vector3 pos0 = centerOfPlayerPos - Vector3.forward;
        pos0.y = 0f;

        Vector3 pos1 = playerToHand;// - centerOfPlayerPos;
        pos1.y = 0f;

        float angle = Vector2.SignedAngle(RemoveYOfVector3(pos0), RemoveYOfVector3(pos1));
        Debug.Log(angle);
		//Quaternion q = CalibrationCursor.ActiveCursorInScene.GetCursorRotation();
		newRot = Quaternion.identity/*q*/ * Quaternion.Euler(new Vector3(0, Mathf.Floor(angle), 0));

        return newRot;
    }

    private Vector2 RemoveYOfVector3(Vector3 v)
    {
        Vector2 returnVal = new Vector2();
        returnVal.x = v.x;
        returnVal.y = v.z;
        return returnVal;
    }

    private float GetAngleBetweenVector2(Vector2 vec1, Vector2 vec2)
    {
        Vector2 diference = vec2 - vec1;
        float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;
        return Vector2.Angle(Vector2.right, diference) * sign;
    }

	//private void OnDrawGizmosSelected()
	//{
	//	if(rigTransform != null)
	//	{
	//		Gizmos.color = Color.blue;
	//		Gizmos.DrawCube(rigTransform.position, new Vector3(0.5f, 10f, 0.5f));
	//		Gizmos.color = Color.yellow;
	//		Gizmos.DrawCube(rigTransform.GetChild(0).position, new Vector3(0.4f, 15f, 0.4f));
	//		Gizmos.color = Color.red;
	//		Gizmos.DrawCube(head.position, new Vector3(0.3f, 20f, 0.3f));
	//	}
	//}
}

