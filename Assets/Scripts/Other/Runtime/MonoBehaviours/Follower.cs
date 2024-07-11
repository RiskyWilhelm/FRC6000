using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed partial class Follower : MonoBehaviour
{
	[Header("Follower Movement")]
	#region Follower Movement

	[SerializeField]
	private Rigidbody2D selfRigidbody;

	[SerializeField]
	private Transform relativeTo;

	public Vector3 offset;

	public UpdateType updateType = UpdateType.Update;


	#endregion


	// Update
	private void Update()
	{
		if (updateType is UpdateType.Update)
			UpdatePosition();
	}

	private void FixedUpdate()
	{
		if (updateType is UpdateType.FixedUpdate)
			UpdatePosition();
	}

	private void LateUpdate()
	{
		if (updateType is UpdateType.LateUpdate)
			UpdatePosition();
	}

	// TODO: A boolean can control if method will control both rotation axis x or z
	private void UpdatePosition()
	{
		selfRigidbody.position = relativeTo.position + offset;
	}
}


#if UNITY_EDITOR

public sealed partial class Follower
{ }

#endif