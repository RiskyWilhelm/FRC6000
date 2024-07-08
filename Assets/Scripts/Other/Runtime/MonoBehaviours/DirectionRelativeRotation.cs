using UnityEngine;

public sealed partial class DirectionRelativeRotation : MonoBehaviour
{
	[Header("DirectionRelativeRotation Movement")]
	#region DirectionRelativeRotation Movement

	public UpdateType updateType = UpdateType.Update;

    private Vector3 lastPosition;


	#endregion


	// Update
	private void Update()
	{
		if (updateType is UpdateType.Update)
			UpdateRotation();
	}

	private void FixedUpdate()
	{
		if (updateType is UpdateType.FixedUpdate)
			UpdateRotation();
	}

	private void LateUpdate()
	{
		if (updateType is UpdateType.LateUpdate)
			UpdateRotation();
	}

	// TODO: A boolean can control if method will control both rotation axis x or z
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