using System;
using UnityEngine;

public sealed partial class DirectionRelativeRotation : MonoBehaviour
{
	[Header("DirectionRelativeRotation Movement")]
	#region DirectionRelativeRotation Movement

	[SerializeField]
	private Transform relativeTo;

	[SerializeField]
	private Transform controlled;

	[SerializeField]
	private bool isReversed;

	[SerializeField]
	private UpdateType updateType = UpdateType.Update;

	[NonSerialized]
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
		// Calculate new dir and others
		var newDir = (relativeTo.position - lastPosition).normalized;

		// Get rotations
		Quaternion rightRotation;
		Quaternion leftRotation;

		if (isReversed)
		{
			rightRotation = Quaternion.Euler(new Vector3(0, 180, 0));
			leftRotation = Quaternion.Euler(new Vector3(0, 0, 0));
		}
		else
		{
			rightRotation = Quaternion.Euler(new Vector3(0, 0, 0));
			leftRotation = Quaternion.Euler(new Vector3(0, 180, 0));
		}

		// Update rotation and values
		if (newDir.x > 0)
			controlled.rotation = rightRotation;
		else if (newDir.x < 0)
			controlled.rotation = leftRotation;

		lastPosition = relativeTo.position;
	}
}


#if UNITY_EDITOR

public sealed partial class DirectionRelativeRotation
{ }

#endif