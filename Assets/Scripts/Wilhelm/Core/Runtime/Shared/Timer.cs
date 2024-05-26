using System;
using UnityEngine;

[Serializable]
public struct Timer : IEquatable<Timer>
{
    private float currentTime;

    public float tickTime;


    // Initialize
    public Timer(float tickTime)
    {
        this.tickTime = tickTime;
        this.currentTime = 0;
    }


	// Update
	/// <summary> Resets when timer ends otherwise ticks </summary>
	/// <returns> true if ended </returns>
	public bool Tick()
    {
        currentTime += Time.deltaTime;
            
        if (currentTime >= tickTime)
        {
            Reset();
            return true;
        }

        return false;
    }

    public void Reset()
    {
        currentTime = 0;
    }

	public override bool Equals(object obj)
	{
		return (obj is Timer timer) && Equals(timer);
	}

	public bool Equals(Timer other)
	{
		return currentTime == other.currentTime &&
			   tickTime == other.tickTime;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(currentTime, tickTime);
	}

	public static bool operator ==(Timer left, Timer right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Timer left, Timer right)
	{
		return !(left == right);
	}
}