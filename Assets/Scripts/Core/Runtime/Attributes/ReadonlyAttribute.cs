using UnityEngine;

/// <summary> Customizes the field shown in inspector </summary>
public sealed partial class ReadonlyAttribute : PropertyAttribute
{ }


#if UNITY_EDITOR

public sealed partial class ReadonlyAttribute
{ }

#endif
