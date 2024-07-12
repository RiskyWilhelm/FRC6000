using UnityEngine;

public static class CircleCollider2DExtensions
{
	public static Vector2 GetRandomPoint(this CircleCollider2D a)
	{
		float radAngle = Random.Range(0, Mathf.PI * 2);
		float distance = a.radius * Mathf.Sqrt(Random.Range(0f, 1f)); // sqrt for even distribution

		Vector2 localPoint = new (distance * Mathf.Cos(radAngle), distance * Mathf.Sin(radAngle));
		localPoint += a.offset;
		return a.transform.TransformPoint(localPoint);
	}
}