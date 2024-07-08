using UnityEngine;

[DisallowMultipleComponent]
public abstract partial class MonoBehaviourSingletonBase<SingletonType> : MonoBehaviour
    where SingletonType : MonoBehaviourSingletonBase<SingletonType>
{
    private static SingletonType _instance;

    public static SingletonType Instance
    {
        get
        {
            if (_instance == null)
                FindOrTryCreateSingleton();

            return _instance;
        }
    }

    public static bool IsAnyInstanceLiving => (_instance != null) || FindFirstObjectByType<SingletonType>(findObjectsInactive: FindObjectsInactive.Include);

	public virtual string GameObjectName => typeof(SingletonType).Name;


	// Initialize
	protected virtual void Awake()
    {
        // If other instance is living
        if ((_instance != null) && (_instance != this))
        {
            DestroyImmediate(this.gameObject);
            return;
        }

        _instance = (this as SingletonType);
    }

    protected static void TryCreateSingleton()
    {
        if (GameControllerPersistentSingleton.IsQuitting || SceneControllerPersistentSingleton.IsActiveSceneChanging)
            throw new System.Exception("Cant create Singleton. You are probably trying to instantiate in OnDestroy() or OnDisable()");

		_instance = new GameObject(typeof(SingletonType).Name, typeof(SingletonType)).GetComponent<SingletonType>();
		_instance.name = _instance.GameObjectName;

#if UNITY_EDITOR
		if (!Application.isPlaying)
			_instance.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
#endif
	}

    protected static void FindOrTryCreateSingleton()
    {
        // Try to find
        if (_instance == null)
            _instance = FindFirstObjectByType<SingletonType>(findObjectsInactive: FindObjectsInactive.Include);

        // If still cant find, try to create
        if (_instance == null)
            TryCreateSingleton();
    }

	protected static void DestroyAllInstances()
    {
        foreach (var iteratedInstance in FindObjectsByType<SingletonType>(FindObjectsInactive.Include, sortMode: FindObjectsSortMode.None))
            DestroyImmediate(iteratedInstance.gameObject);
    }
}


#if UNITY_EDITOR

public abstract partial class MonoBehaviourSingletonBase<SingletonType>
{ }

#endif