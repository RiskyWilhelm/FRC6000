// I always like to split up the development into two parts. In this file, i introduce my coding style to you
using UnityEngine;

// Runtime side
public sealed partial class MonoBehaviourExample : MonoBehaviour
{
	private int someField;

    // Use prefix _
    private int _somePropertyBackend;
    private int _somePropertyBackend2;

    public int SomePropertyBackend
    {
        get
		{
			return _somePropertyBackend;
		}

        set
		{
			_somePropertyBackend = value;
		}
    }

    public int SomePropertyBackend2 => _somePropertyBackend2;


	// Initialize
	private void Start()
	{
		
	}

	public void SomeInitializationMethod()
	{

	}


	// Update
	private void Update()
	{
		MethodCalledInUpdate();	
	}

	// Topic is always here. Not in the Update method directly
	public void MethodCalledInUpdate()
	{

	}

	public void AMethodWhichCalledUnknownAmountOfTimes()
	{

	}


	// Dispose
	private void OnDestroy()
	{
		SomeDestructionMethod();
	}

	private void OnDisable()
	{

	}

	public void SomeDestructionMethod()
	{
		
	}
}


// Editor Side
#if UNITY_EDITOR

public sealed partial class MonoBehaviourExample
{
	// e_ Should be there in order to let coder understand it is an editor variable
	public int e_PutPrefixForEverything;

	public void E_PutPrefixForMethods()
    { }
}

#endif