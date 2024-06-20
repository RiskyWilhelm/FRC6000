using System;
using UnityEngine;

[Serializable]
public struct Timer : IEquatable<Timer>
{
	[Tooltip("If isRandomized set to true, used to set TickTime on Reset() or when the timer finishes")]
	public float minInclusiveTickSeconds;

	[Tooltip("If isRandomized set to true, used to set TickTime on Reset() or when the timer finishes")]
	public float maxInclusiveTickSeconds;

	private float currentSecond;

    public float tickSecond;

	public bool isRandomized;

	private static readonly System.Random mainRandomizer = new ();


    // Initialize
    public Timer(float tickTime)
    {
		this.minInclusiveTickSeconds = 0;
		this.maxInclusiveTickSeconds = 0;
        this.tickSecond = tickTime;
        this.currentSecond = 0;
		this.isRandomized = false;
    }

	public Timer(float tickTime, float minInclusiveTickTime, float maxInclusiveTickTime, bool isRandomized = true)
	{
		this.minInclusiveTickSeconds = minInclusiveTickTime;
		this.maxInclusiveTickSeconds = maxInclusiveTickTime;

		if (isRandomized)
			this.tickSecond = Mathf.Abs((float)mainRandomizer.NextDouble(minInclusiveTickTime, maxInclusiveTickTime));
		else
			this.tickSecond = tickTime;

		this.currentSecond = 0;
		this.isRandomized = isRandomized;
	}

	public Timer(float minInclusiveTickTime, float maxInclusiveTickTime)
	{
		this.minInclusiveTickSeconds = minInclusiveTickTime;
		this.maxInclusiveTickSeconds = maxInclusiveTickTime;
		this.tickSecond = Mathf.Abs((float)mainRandomizer.NextDouble(minInclusiveTickTime, maxInclusiveTickTime));
		this.currentSecond = 0;
		this.isRandomized = true;
	}


	// Update
	/// <summary> Resets when timer ends otherwise ticks </summary>
	/// <returns> true if ended </returns>
	public bool Tick()
    {
        currentSecond += Time.deltaTime;
            
        if (currentSecond >= tickSecond)
        {
            Reset();
            return true;
        }

        return false;
    }

    public void Reset()
    {
		if (isRandomized)
			tickSecond = Mathf.Abs(UnityEngine.Random.Range(minInclusiveTickSeconds, maxInclusiveTickSeconds));

        currentSecond = 0;
    }

	public override bool Equals(object obj)
	{
		return (obj is Timer timer) && Equals(timer);
	}

	public bool Equals(Timer other)
	{
		return currentSecond == other.currentSecond &&
			   tickSecond == other.tickSecond;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(currentSecond, tickSecond);
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