using System;
using UnityEngine;

public interface IFrameDependentPhysicsInteractor
{
	/// <summary> Must not be used in MonoBehaviour.FixedUpdate() </summary>
	public void DoFrameDependentPhysics();
}

public interface IFrameDependentPhysicsInteractor<TriggerTypeEnum> : IFrameDependentPhysicsInteractor
	where TriggerTypeEnum : Enum
{
	public void RegisterFrameDependentPhysicsInteraction((TriggerTypeEnum triggerType, Collider2D collider2D, Collision2D collision2D) interaction);
}