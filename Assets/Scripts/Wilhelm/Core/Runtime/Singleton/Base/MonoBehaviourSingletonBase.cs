using UnityEngine;

public abstract partial class MonoBehaviourSingletonBase<UObject> : MonoBehaviour
    where UObject : MonoBehaviourSingletonBase<UObject>
{
    public static UObject instance;


    // Initialize
    protected virtual void Awake()
    {
        // If other instance is living
        if (instance)
        {
            DestroyImmediate(this.gameObject);
            return;
        }

        instance = (this as UObject);
    }
}


#if UNITY_EDITOR

public abstract partial class MonoBehaviourSingletonBase<UObject>
{ }

#endif