using System;
using UnityEngine;

[Serializable]
public struct Timer : IEquatable<Timer>, IEquatable<TimerRandomized>
{
	[SerializeField]
	private TimeType _tickType;

	[SerializeField]
	private float _tickSecond;

	private float _currentSecond;

	public readonly float CurrentSecond => _currentSecond;

	public readonly bool HasEnded => (_currentSecond >= _tickSecond);

	public TimeType TickType
	{
		readonly get => _tickType;
		set => _tickType = value;
	}

	public float TickSecond
	{
		readonly get => _tickSecond;
		set => _tickSecond = value;
	}


    // Initialize
    public Timer(float tickSecond, TimeType tickType = TimeType.Scaled)
    {
        this._tickSecond = tickSecond;
        this._currentSecond = 0;
		this._tickType = tickType;
    }


	// Update
	/// <returns> true if timer has ended </returns>
	public bool Tick()
    {
		if (_currentSecond < _tickSecond)
		{
			switch (_tickType)
			{
				case TimeType.Scaled:
				_currentSecond += Time.deltaTime;
				break;

				case TimeType.Unscaled:
				_currentSecond += Time.unscaledDeltaTime;
				break;

				default:
					goto case TimeType.Scaled;
			}
		}

        return _currentSecond >= _tickSecond;
    }

	/// <summary> Sets the <see cref="_currentSecond"/> to zero </summary>
	public void Reset()
    {
        _currentSecond = 0;
    }

	public override bool Equals(object obj)
	{
		if (obj is Timer timer)
			return Equals(timer);

		if (obj is TimerRandomized randomized)
			return Equals(randomized);

		return false;
	}

	public bool Equals(Timer other)
	{
		return (_tickType, _currentSecond, _tickSecond) == (other._tickType, other._currentSecond, other._tickSecond);
	}

	public bool Equals(TimerRandomized other)
	{
		return (_tickType, _currentSecond, _tickSecond) == (other.TickType, other.CurrentSecond, other.TickSecond);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(_tickType, _currentSecond, _tickSecond);
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