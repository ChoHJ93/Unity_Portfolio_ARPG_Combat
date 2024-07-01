using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T m_Instance = null;
    public static T Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = GameObject.FindObjectOfType(typeof(T)) as T;
                if (m_Instance == null)
                {
                    GameObject obj = new GameObject(string.Format($"##Singleton_{typeof(T)}"));
                    m_Instance = obj.AddComponent(typeof(T)) as T;
#if UNITY_EDITOR
                    if (Application.isPlaying)
#endif
                        DontDestroyOnLoad(m_Instance.gameObject);
                }
            }
            return m_Instance;
        }
    }

    protected virtual void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (null == m_Instance)
            m_Instance = this as T;
    }

    public virtual void Initialize() { }

    public virtual void Clear() { }

    protected virtual void OnDestroy()
    {
        if (null != m_Instance)
        {
            Clear();
            m_Instance = null;
        }
    }

    private void OnApplicationQuit()
    {
        m_Instance = null;
    }
}
