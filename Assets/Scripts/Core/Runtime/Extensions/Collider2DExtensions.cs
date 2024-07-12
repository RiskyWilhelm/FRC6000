using UnityEngine;

public static class Collider2DExtensions
{
	public static Vector2 GetRandomPoint(this Collider2D a)
	{
        switch (a)
		{
			case BoxCollider2D:
			return (a as BoxCollider2D).GetRandomPoint();

			case CircleCollider2D:
			return (a as CircleCollider2D).GetRandomPoint();
		}

		Debug.LogErrorFormat("Type {0} is un-supported. BoxCollider2D and CircleCollider2D is supported only", a.GetType());
		return default;
	}
}