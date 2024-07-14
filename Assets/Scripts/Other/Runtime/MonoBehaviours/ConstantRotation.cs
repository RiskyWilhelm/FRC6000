using UnityEngine;

public sealed partial class ConstantRotation : MonoBehaviour
{
	[Header("DirectionRelativeRotation Movement")]
	#region DirectionRelativeRotation Movement

	[SerializeField]
	private Transform controlled;

	[SerializeField]
	private UpdateType updateType = UpdateType.Update;

	[SerializeField]
	[VectorRange(minX: -1f, maxX: 1f, minY: -1f, maxY: 1f, minZ: -1f, maxZ: 1f)]
	private Vector3 rotationEuler;


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
		controlled.rotation = Quaternion.RotateTowards(controlled.rotation, Quaternion.Euler(rotationEuler), 360f);
	}
}


#if UNITY_EDITOR

public sealed partial class ConstantRotation
{ }

#endif