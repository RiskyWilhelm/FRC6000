using UnityEngine;

public interface ICarrier
{
	public void Carry(ICarryable carryable, Rigidbody2D carryableRigidbody);

	public void StopCarrying();
	
	public void StopCarrying(ICarryable carryable);
}