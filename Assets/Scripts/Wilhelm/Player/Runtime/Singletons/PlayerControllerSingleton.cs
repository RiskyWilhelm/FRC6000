using System;
using System.Collections.Generic;

public sealed partial class PlayerControllerSingleton : MonoBehaviourSingletonBase<PlayerControllerSingleton>
{
	#region PlayerControllerSingleton Events

	private static readonly Dictionary<TargetType, Action> _onTargetBirthEventDict = new();

	private static readonly Dictionary<TargetType, Action> _onTargetDeathEventDict = new();

	public static Dictionary<TargetType, Action> onTargetBirthEventDict
	{
		get
		{
			if (_onTargetBirthEventDict.Count == 0)
			{
				foreach (TargetType iteratedTargetType in Enum.GetValues(typeof(TargetType)))
					_onTargetBirthEventDict.TryAdd(iteratedTargetType, null);
			}

			return _onTargetBirthEventDict;
		}
	}

	public static Dictionary<TargetType, Action> onTargetDeathEventDict
	{
		get
		{
			if (_onTargetDeathEventDict.Count == 0)
			{
				foreach (TargetType iteratedTargetType in Enum.GetValues(typeof(TargetType)))
					_onTargetDeathEventDict.TryAdd(iteratedTargetType, null);
			}

			return _onTargetDeathEventDict;
		}
	}


	#endregion
}


#if UNITY_EDITOR

public sealed partial class PlayerControllerSingleton
{ }

#endif