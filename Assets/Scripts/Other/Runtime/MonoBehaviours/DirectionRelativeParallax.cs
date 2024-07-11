using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed partial class DirectionRelativeParallax : MonoBehaviour
{
	[Header("DirectionRelativeParallax Movement")]
	#region DirectionRelativeParallax Movement

	[SerializeField]
	private Rigidbody2D selfRigidbody;

    [SerializeField]
    private Transform relativeTo;

	public UpdateType updateType = UpdateType.Update;


	#endregion

	[Header("DirectionRelativeParallax Parallax")]
	#region DirectionRelativeParallax Parallax

	[SerializeField]
	private bool isParallaxHorizontallyInfinite;

	[SerializeField]
	private bool isParallaxVerticallyInfinite;

	[SerializeField]
	private bool isParallaxBoundsFromSpriteRenderer;

	[SerializeField]
	private Vector2 parallaxBounds;

	[SerializeField]
	[VectorRange(minX: 0f, maxX: 1f, minY: 0f, maxY: 1f)]
	private Vector2 parallaxStrength;

	[NonSerialized]
	private Vector2 persistentParallaxPosition;


	#endregion



	// Initialize
	private void Awake()
	{
		persistentParallaxPosition = this.transform.position;

		if (isParallaxBoundsFromSpriteRenderer)
			parallaxBounds = GetComponent<SpriteRenderer>().bounds.size;

		DoParallax();
	}


	// Update
	private void Update()
	{
		if (updateType is UpdateType.Update)
			DoParallax();
	}

	private void FixedUpdate()
	{
		if (updateType is UpdateType.FixedUpdate)
			DoParallax();
	}

	private void LateUpdate()
	{
		if (updateType is UpdateType.LateUpdate)
			DoParallax();
	}

	private void DoParallax()
	{
		if (isParallaxHorizontallyInfinite || isParallaxVerticallyInfinite)
			UpdatePositionsForInfiniteParallax();

		var parallaxedPositionRelativeToTarget = (relativeTo.position * parallaxStrength);
		var parallaxedPosition = persistentParallaxPosition + parallaxedPositionRelativeToTarget;

		selfRigidbody.position = parallaxedPosition;
	}

	private void UpdatePositionsForInfiniteParallax()
	{
		var movedPosition = (relativeTo.position * (Vector2.one - parallaxStrength)); // How far relative object moved?

		if (isParallaxHorizontallyInfinite)
		{
			// Check for positive
			if (movedPosition.x > (persistentParallaxPosition.x + parallaxBounds.x))
				persistentParallaxPosition.x += parallaxBounds.x;
			// Check for negative
			else if (movedPosition.x < (persistentParallaxPosition.x - parallaxBounds.x))
				persistentParallaxPosition.x -= parallaxBounds.x;
		}

		if (isParallaxVerticallyInfinite)
		{
			// Check for positive
			if (movedPosition.y > (persistentParallaxPosition.y + parallaxBounds.y))
				persistentParallaxPosition.y += parallaxBounds.y;
			// Check for negative
			else if (movedPosition.y < (persistentParallaxPosition.y - parallaxBounds.y))
				persistentParallaxPosition.y -= parallaxBounds.y;
		}
	}
}


#if UNITY_EDITOR

public sealed partial class DirectionRelativeParallax
{
	private void OnDrawGizmosSelected()
	{
		DrawParallaxBounds();
	}

	private void DrawParallaxBounds()
	{
		Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
		Gizmos.DrawCube(selfRigidbody.position, parallaxBounds);
	}
}

#endif