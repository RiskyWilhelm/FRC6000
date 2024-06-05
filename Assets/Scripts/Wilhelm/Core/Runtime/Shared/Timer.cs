using System;
using UnityEngine;

[Serializable]
public struct Timer : IEquatable<Timer>
{
	[Tooltip("If isRandomized set to true, used to set TickTime on Reset() or when the timer finishes")]
	public float minInclusiveTickTime;

	[Tooltip("If isRandomized set to true, used to set TickTime on Reset() or when the timer finishes")]
	public float maxInclusiveTickTime;

	private float currentTime;

    public float tickTime;

	public bool isRandomized;

	private static System.Random mainRandomizer = new ();


    // Initialize
    public Timer(float tickTime)
    {
		this.minInclusiveTickTime = 0;
		this.maxInclusiveTickTime = 0;
        this.tickTime = tickTime;
        this.currentTime = 0;
		this.isRandomized = false;
    }

	public Timer(float tickTime, float minInclusiveTickTime, float maxInclusiveTickTime, bool isRandomized = true)
	{
		this.minInclusiveTickTime = minInclusiveTickTime;
		this.maxInclusiveTickTime = maxInclusiveTickTime;

		if (isRandomized)
			this.tickTime = Mathf.Abs((float)mainRandomizer.NextDouble(minInclusiveTickTime, maxInclusiveTickTime));
		else
			this.tickTime = tickTime;

		this.currentTime = 0;
		this.isRandomized = isRandomized;
	}

	public Timer(float minInclusiveTickTime, float maxInclusiveTickTime)
	{
		this.minInclusiveTickTime = minInclusiveTickTime;
		this.maxInclusiveTickTime = maxInclusiveTickTime;
		this.tickTime = Mathf.Abs((float)mainRandomizer.NextDouble(minInclusiveTickTime, maxInclusiveTickTime));
		this.currentTime = 0;
		this.isRandomized = true;
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
		if (isRandomized)
			tickTime = Mathf.Abs(UnityEngine.Random.Range(minInclusiveTickTime, maxInclusiveTickTime));

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