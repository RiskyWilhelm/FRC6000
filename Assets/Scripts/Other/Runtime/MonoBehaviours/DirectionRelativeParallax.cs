using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed partial class DirectionRelativeParallax : MonoBehaviour
{
	[Header("DirectionRelativeParallax Movement")]
	[SerializeField]
	private Rigidbody2D selfRigidbody;

    [SerializeField]
    private Transform relativeTo;

	[SerializeField]
	private float strength;

	[NonSerialized]
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
		selfRigidbody.position = new Vector2(initialPosition.x + (strength * distanceToRelativeObject), selfRigidbody.position.y);
	}
}


#if UNITY_EDITOR

public sealed partial class DirectionRelativeParallax
{ }

#endif