using UnityEngine;

public abstract partial class MonoBehaviourSingletonBase<T> : MonoBehaviour
    where T : MonoBehaviourSingletonBase<T>
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (!_instance)
                FindOrCreate();

            return _instance;
        }
    }

    public static bool IsInstanceLiving => _instance;

    protected virtual string GameObjectName => typeof(T).Name;


	// Initialize
	protected virtual void Awake()
    {
        // If other instance is living
        if (Instance && (_instance != this))
        {
            DestroyImmediate(this.gameObject);
            return;
        }

        _instance = (this as T);
    }

	protected static void FindOrCreate()
	{
        // Try to find
		_instance = FindFirstObjectByType<T>(findObjectsInactive: FindObjectsInactive.Include);

        // If still cant find, try to create
        if (!_instance)
        {
            var newSingleton = new GameObject(typeof(T).Name, typeof(T)).GetComponent<T>();
            newSingleton.name = newSingleton.GameObjectName;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                newSingleton.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
#endif
        }
	}
}


#if UNITY_EDITOR

public abstract partial class MonoBehaviourSingletonBase<T>
{ }

#endif