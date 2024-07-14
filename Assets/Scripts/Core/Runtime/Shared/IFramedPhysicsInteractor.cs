using System;
using UnityEngine;

public interface IFrameDependentPhysicsInteractor
{
	/// <summary> Must not be used in MonoBehaviour.FixedUpdate() </summary>
	public void DoFrameDependentPhysics();
}

public interface IFrameDependentPhysicsInteractor<CollideTypeEnum> : IFrameDependentPhysicsInteractor
	where CollideTypeEnum : Enum
{
	public void RegisterFrameDependentPhysicsInteraction((CollideTypeEnum triggerType, Collider2D collider2D, Collision2D collision2D) interaction);
}