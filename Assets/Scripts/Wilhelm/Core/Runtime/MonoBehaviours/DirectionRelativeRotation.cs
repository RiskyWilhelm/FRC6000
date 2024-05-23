using UnityEngine;

public sealed partial class DirectionRelativeRotation : MonoBehaviour
{
	// Movement
	public UpdateType updateType;

    private Vector3 lastPosition;


	// Update
	private void Update()
	{
		if (updateType == UpdateType.Update)
			UpdateRotation();
	}

	private void FixedUpdate()
	{
		if (updateType == UpdateType.FixedUpdate)
			UpdateRotation();
	}

	private void LateUpdate()
	{
		if (updateType == UpdateType.LateUpdate)
			UpdateRotation();
	}

	private void UpdateRotation()
	{
		// Calculate new dir
		var newDir = (this.transform.position - lastPosition).normalized;

		// Update rotation and values
		if (newDir.x > 0)
			this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
		else if (newDir.x < 0)
			this.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));

		lastPosition = this.transform.position;
	}
}


#if UNITY_EDITOR

public sealed partial class DirectionRelativeRotation
{ }

#endif