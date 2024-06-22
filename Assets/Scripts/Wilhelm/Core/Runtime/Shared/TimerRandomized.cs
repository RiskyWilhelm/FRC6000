using System;
using UnityEngine;

[Serializable]
public struct TimerRandomized : IEquatable<TimerRandomized>, IEquatable<Timer>
{
	private float _currentSecond;

	[SerializeField]
	private float _tickSecond;

	[SerializeField]
	private float _minInclusiveTickSeconds;

	[SerializeField]
	private float _maxExclusiveTickSeconds;

	public readonly float CurrentSecond
	{
		get => _currentSecond;
	}

	public readonly bool HasEnded => (_currentSecond >= _tickSecond);

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
	public TimerRandomized(float tickSecond)
	{
		this._minInclusiveTickSeconds = 0;
		this._maxExclusiveTickSeconds = 0;
		this._tickSecond = tickSecond;
		this._currentSecond = 0;
	}

	public TimerRandomized(float tickSecond, float minInclusiveTickTime, float maxExclusiveTickTime)
	{
		this._minInclusiveTickSeconds = minInclusiveTickTime;
		this._maxExclusiveTickSeconds = maxExclusiveTickTime;
		this._tickSecond = tickSecond;
		this._currentSecond = 0;
	}

	public TimerRandomized(float minInclusiveTickTime, float maxExclusiveTickTime)
	{
		this._minInclusiveTickSeconds = minInclusiveTickTime;
		this._maxExclusiveTickSeconds = maxExclusiveTickTime;
		this._tickSecond = Mathf.Abs(randomizer.NextFloat(_minInclusiveTickSeconds, _maxExclusiveTickSeconds));
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

	/// <summary> Sets the <see cref="_tickSecond"/> around <see cref="_minInclusiveTickSeconds"/> and <see cref="_maxExclusiveTickSeconds"/> </summary>
	public void Randomize()
	{
		_tickSecond = Mathf.Abs(randomizer.NextFloat(_minInclusiveTickSeconds, _maxExclusiveTickSeconds));
	}

	/// <summary> Shortcut to <see cref="Reset"/> and <see cref="Randomize"/> calls </summary>
	public void ResetAndRandomize()
	{
		Reset();
		Randomize();
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
		return (_currentSecond, _tickSecond, _minInclusiveTickSeconds, _maxExclusiveTickSeconds) == (other._currentSecond, other._tickSecond, other._minInclusiveTickSeconds, other._maxExclusiveTickSeconds);
	}

	public bool Equals(Timer other)
	{
		return (_currentSecond, _tickSecond) == (other.CurrentSecond, other.TickSecond);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(_minInclusiveTickSeconds, _maxExclusiveTickSeconds, _currentSecond, _tickSecond);
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