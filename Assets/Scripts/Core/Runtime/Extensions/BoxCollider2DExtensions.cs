using UnityEngine;

public static class BoxCollider2DExtensions
{
	public static Vector2 GetRandomPoint(this BoxCollider2D a)
	{
		Vector2 extents = a.bounds.extents;

		Vector2 localPoint = new (
			Random.Range(-extents.x, extents.x),
			Random.Range(-extents.y, extents.y)
		);

		localPoint += a.offset;
		return a.transform.TransformPoint(localPoint);
	}
}