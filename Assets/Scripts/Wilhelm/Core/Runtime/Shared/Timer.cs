using System;
using UnityEngine;

[Serializable]
public struct Timer : IEquatable<Timer>, IEquatable<TimerRandomized>
{
	private float _currentSecond;

	[SerializeField]
    private float _tickSecond;

	public readonly float CurrentSecond => _currentSecond;

	public readonly bool HasEnded => (_currentSecond >= _tickSecond);

	public float TickSecond
	{
		readonly get => _tickSecond;
		set => _tickSecond = value;
	}


    // Initialize
    public Timer(float tickSecond)
    {
        this._tickSecond = tickSecond;
        this._currentSecond = 0;
    }


	// Update
	/// <returns> true if timer has ended </returns>
	public bool Tick()
    {
		if (_currentSecond < _tickSecond)
			_currentSecond += Time.deltaTime;

        return _currentSecond >= _tickSecond;
    }

	/// <summary> Sets the <see cref="_currentSecond"/> to zero </summary>
	public void Reset()
    {
        _currentSecond = 0;
    }

	public override bool Equals(object obj)
	{
		if (obj is TimerRandomized randomized)
			return Equals(randomized);

		if (obj is Timer timer)
			return Equals(timer);

		return false;
	}

	public bool Equals(Timer other)
	{
		return (_currentSecond, _tickSecond) == (other._currentSecond, other._tickSecond);
	}

	public bool Equals(TimerRandomized other)
	{
		return (_currentSecond, _tickSecond) == (other.CurrentSecond, other.TickSecond);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(_currentSecond, _tickSecond);
	}

	public static bool operator ==(Timer left, Timer right)
	{
		return left.Equals(right);
	}

	public static bool operator ==(Timer left, TimerRandomized right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Timer left, Timer right)
	{
		return !(left == right);
	}

	public static bool operator !=(Timer left, TimerRandomized right)
	{
		return !(left == right);
	}
}