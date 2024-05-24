using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed partial class DirectionRelativeParallax : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField]
	private Rigidbody2D selfRigidbody;

    [SerializeField]
    private Transform relativeTo;

	[SerializeField]
	private float strength;

	private Vector2 initialPosition;


	// Initialize
	private void Awake()
	{
		initialPosition = this.transform.position;
		DoParallax();
	}


	// Update
	private void FixedUpdate()
	{
		DoParallax();
	}

	private void DoParallax()
	{
		var distanceToRelativeObject = (relativeTo.position.x - initialPosition.x);
		selfRigidbody.MovePosition(new Vector2(initialPosition.x + (strength * distanceToRelativeObject), selfRigidbody.position.y));
	}
}


#if UNITY_EDITOR

public sealed partial class DirectionRelativeParallax
{ }

#endif