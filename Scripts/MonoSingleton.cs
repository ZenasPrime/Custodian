using UnityEngine;

namespace ZenTools.Custodian
{
    /// <summary>
    /// A thread-safe, generic singleton class for MonoBehaviours in Unity. 
    /// Ensures that only one instance of the MonoBehaviour exists and optionally persists across scenes.
    /// </summary>
    /// <typeparam name="T">Type of MonoBehaviour that will be a singleton.</typeparam>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static object _lock = new object();
        private static bool applicationIsQuitting = false;

        [SerializeField] private bool persistAcrossScenes = false;

        /// <summary>
        /// Provides a global access point to the singleton instance. Handles lazy instantiation and ensures
        /// that only one instance exists. Returns null if the application is quitting to avoid creation during shutdown.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));

                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogError($"[Singleton] Something went wrong - there should never be more than 1 singleton! Reopening the scene might fix it.");
                            return _instance;
                        }

                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"{typeof(T).ToString()} (Singleton)";

                            if (_instance.GetComponent<MonoSingleton<T>>().persistAcrossScenes)
                            {
                                DontDestroyOnLoad(singletonObject);
                            }
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// Ensures that the singleton instance is correctly assigned and optionally persists across scenes.
        /// Destroys the gameObject if a duplicate instance is detected.
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (persistAcrossScenes)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Marks the singleton as quitting when the instance is destroyed to prevent recreation during application shutdown.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                applicationIsQuitting = true;
            }
        }
    }
}
