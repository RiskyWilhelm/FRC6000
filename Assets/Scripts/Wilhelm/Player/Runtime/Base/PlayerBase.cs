using UnityEngine;

public abstract partial class PlayerBase : MonoBehaviour
{
	[Header("PlayerBase Movement")]
	#region PlayerBase Movement

	[SerializeField]
	protected Rigidbody2D selfRigidbody;

	[SerializeField]
	[Tooltip("You should set this nearly but not same as the collider size")]
	protected Vector2 raycastBounds = new(1f, 1f);


	#endregion


	// Update
	public bool IsGrounded()
		=> IsGroundedAtVector(selfRigidbody.position);

	public bool IsGroundedAtVector(Vector2 worldPosition)
	{
		// Bounds * 0.5f gets the extents (half size)
		var groundRaycast = Physics2D.BoxCast(worldPosition, new Vector2(raycastBounds.x, 0.5f), 0, Vector2.down, (raycastBounds * 0.5f).y, Layers.Mask.Ground);
		return groundRaycast.collider;
	}
}


#if UNITY_EDITOR

public abstract partial class PlayerBase
{ }

#endif