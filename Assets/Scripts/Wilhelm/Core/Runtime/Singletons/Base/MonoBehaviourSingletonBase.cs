using UnityEngine;

public abstract partial class MonoBehaviourSingletonBase<SingletonType> : MonoBehaviour
    where SingletonType : MonoBehaviourSingletonBase<SingletonType>
{
    private static SingletonType _instance;

    public static SingletonType Instance
    {
        get
        {
            if (!_instance)
                FindOrCreate();

            return _instance;
        }
    }

    public static bool IsInstanceLiving => _instance;

    protected virtual string GameObjectName => typeof(SingletonType).Name;


	// Initialize
	protected virtual void Awake()
    {
        // If other instance is living
        if (Instance && (_instance != this))
        {
            DestroyImmediate(this.gameObject);
            return;
        }

        _instance = (this as SingletonType);
    }

	protected static void FindOrCreate()
	{
        // Try to find
		_instance = FindFirstObjectByType<SingletonType>(findObjectsInactive: FindObjectsInactive.Include);

        // If still cant find, try to create
        if (!_instance)
        {
            var newSingleton = new GameObject(typeof(SingletonType).Name, typeof(SingletonType)).GetComponent<SingletonType>();
            newSingleton.name = newSingleton.GameObjectName;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                newSingleton.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
#endif
        }
	}
}


#if UNITY_EDITOR

public abstract partial class MonoBehaviourSingletonBase<SingletonType>
{ }

#endif