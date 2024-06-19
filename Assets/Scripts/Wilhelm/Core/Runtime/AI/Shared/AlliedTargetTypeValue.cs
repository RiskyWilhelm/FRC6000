using System;

/// <summary> Stores target type name and it's ally count </summary>
[Serializable]
public struct AlliedTargetTypeValue : IEquatable<AlliedTargetTypeValue>
{
    public TargetType targetTag;

	public uint allyCount;

	public AlliedTargetTypeValue(TargetType targetTag, uint allyCount)
	{
		this.targetTag = targetTag;
		this.allyCount = allyCount;
	}

	public override bool Equals(object obj)
	{
		return obj is AlliedTargetTypeValue value && Equals(value);
	}

	public bool Equals(AlliedTargetTypeValue other)
	{
		return targetTag == other.targetTag &&
			   allyCount == other.allyCount;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(targetTag, allyCount);
	}

	public static bool operator ==(AlliedTargetTypeValue left, AlliedTargetTypeValue right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(AlliedTargetTypeValue left, AlliedTargetTypeValue right)
	{
		return !(left == right);
	}
}