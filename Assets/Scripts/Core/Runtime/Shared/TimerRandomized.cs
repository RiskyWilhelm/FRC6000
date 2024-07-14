using System;
using UnityEngine;

[Serializable]
public struct TimerRandomized : IEquatable<TimerRandomized>, IEquatable<Timer>
{
	[SerializeField]
	private TimeType _tickType;

	[SerializeField]
	private float _tickSecond;

	[SerializeField]
	private float _minInclusiveTickSeconds;

	[SerializeField]
	private float _maxExclusiveTickSeconds;

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

	public float MinInclusiveTickSeconds
	{
		readonly get => _minInclusiveTickSeconds;
		set => _minInclusiveTickSeconds = value;
	}

	public float MaxInclusiveTickSeconds
	{
		readonly get => _maxExclusiveTickSeconds;
		set => _maxExclusiveTickSeconds = value;
	}

	public static readonly System.Random randomizer = new();


	// Initialize
	public TimerRandomized(float tickSecond, TimeType tickType = TimeType.Scaled)
	{
		this._minInclusiveTickSeconds = 0f;
		this._maxExclusiveTickSeconds = 0f;
		this._tickSecond = tickSecond;
		this._currentSecond = tickSecond;
		this._tickType = tickType;
	}

	public TimerRandomized(float tickSecond, float minInclusiveTickTime, float maxExclusiveTickTime, TimeType tickType = TimeType.Scaled)
		: this(tickSecond, tickType)
	{
		this._minInclusiveTickSeconds = minInclusiveTickTime;
		this._maxExclusiveTickSeconds = maxExclusiveTickTime;
	}

	public TimerRandomized(float minInclusiveTickTime, float maxExclusiveTickTime, TimeType tickType = TimeType.Scaled)
		: this(Mathf.Abs(randomizer.NextFloat(minInclusiveTickTime, maxExclusiveTickTime)), minInclusiveTickTime, maxExclusiveTickTime, tickType)
	{ }


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

	public void Randomize()
	{
		_tickSecond = Mathf.Abs(randomizer.NextFloat(_minInclusiveTickSeconds, _maxExclusiveTickSeconds));
	}

	/// <summary> Shortcut to <see cref="Randomize"/> and <see cref="Reset"/> calls </summary>
	public void ResetAndRandomize()
	{
		Randomize();
		Reset();
	}

	public override bool Equals(object obj)
	{
		if (obj is TimerRandomized randomized)
			return Equals(randomized);

		if (obj is Timer timer)
			return Equals(timer);

		return false;
	}

	public bool Equals(TimerRandomized other)
	{
		return (_tickType, _currentSecond, _tickSecond, _minInclusiveTickSeconds, _maxExclusiveTickSeconds) == (other._tickType, other._currentSecond, other._tickSecond, other._minInclusiveTickSeconds, other._maxExclusiveTickSeconds);
	}

	public bool Equals(Timer other)
	{
		return (_tickType, _currentSecond, _tickSecond) == (other.TickType, other.CurrentSecond, other.TickSecond);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(_tickType, _currentSecond, _tickSecond, _minInclusiveTickSeconds, _maxExclusiveTickSeconds);
	}

	public static bool operator ==(TimerRandomized left, TimerRandomized right)
	{
		return left.Equals(right);
	}

	public static bool operator ==(TimerRandomized left, Timer right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(TimerRandomized left, TimerRandomized right)
	{
		return !(left == right);
	}

	public static bool operator !=(TimerRandomized left, Timer right)
	{
		return !(left == right);
	}
}