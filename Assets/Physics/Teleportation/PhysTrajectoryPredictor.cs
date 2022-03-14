using UnityEngine;

public class PhysTrajectoryPredictor
{
	private readonly int stepCount;
	private readonly float timePerStep;
	private readonly Vector3 gravity;
	private readonly float launchSpeed;
	private readonly bool checkCollision;
	private readonly int collisionLayerMask;

	private readonly Vector3[] predictedPositions;
	private bool hasCollision;
	private RaycastHit raycastHit;

	public Vector3[] PredictedPositions { get { return predictedPositions; } }
	public bool HasCollision { get { return hasCollision; } }
	public RaycastHit RaycastHit { get { return raycastHit; } }

	public PhysTrajectoryPredictor(int stepCount, float timePerStep, Vector3 gravity, float launchSpeed, bool checkCollision = true, int collisionLayerMask = -1)
	{
		this.stepCount = stepCount;
		this.timePerStep = timePerStep;
		this.gravity = gravity;
		this.launchSpeed = launchSpeed;
		this.checkCollision = checkCollision;
		this.collisionLayerMask = collisionLayerMask;

		this.predictedPositions = new Vector3[stepCount + 1];
	}

	public void Predict(Vector3 position, Vector3 direction)
	{
		UpdateStepPositions(position, direction);
		if(checkCollision)
		{
			UpdateCollisions();
		}
	}

	private void UpdateStepPositions(Vector3 position, Vector3 direction)
	{
		predictedPositions[0] = position;
		Vector3 velocity = direction * launchSpeed;

		for(int i = 0; i < stepCount; i++)
		{
			Vector3 newPosition = position + velocity * timePerStep + 0.2f * gravity * timePerStep * timePerStep;
			position = newPosition;
			velocity = velocity + gravity * timePerStep;

			predictedPositions[i + 1] = newPosition;
		}
	}

	private void UpdateCollisions()
	{
		for(int i = 0; i < stepCount; i++)
		{
			Vector3 start = predictedPositions[i];
			Vector3 end = predictedPositions[i + 1];
			hasCollision = Physics.Raycast(start, end - start, out raycastHit, Vector3.Distance(start, end), collisionLayerMask);
			if(hasCollision) break;
		}
	}
}
