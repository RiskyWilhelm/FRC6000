using System;
using UnityEngine;

[Serializable]
public struct Timer : IEquatable<Timer>, IEquatable<TimerRandomized>
{
	[SerializeField]
	private TimeType _tickType;

	[SerializeField]
	private float _tickSecond;

	[SerializeField]
	private float _currentSecond;

	public float CurrentSecond
	{
		readonly get => _currentSecond;
		set => _currentSecond = value;
	}

	public readonly bool HasEnded => (_currentSecond == 0f);

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
        this._currentSecond = tickSecond;
		this._tickType = tickType;
    }


	// Update
	/// <returns> true if timer has ended </returns>
	public bool Tick()
    {
		if (_currentSecond > 0f)
		{
			switch (_tickType)
			{
				case TimeType.Scaled:
				_currentSecond -= Time.deltaTime;
				break;

				case TimeType.Unscaled:
				_currentSecond -= Time.unscaledDeltaTime;
				break;

				default:
					goto case TimeType.Scaled;
			}
		}
		
		if (_currentSecond <= 0f)
		{
			_currentSecond = 0f;
			return true;
		}

        return false;
    }

	public void Finish()
	{
		_currentSecond = 0f;
	}

	public void Reset()
    {
        _currentSecond = _tickSecond;
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