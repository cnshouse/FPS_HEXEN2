using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class bl_ObjectPooling : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("RegistreOnStart")]
    private PreRegister[] pooledPrefabs;

    private List<PoolObject> m_poolList = new List<PoolObject>();

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        for (int i = 0; i < pooledPrefabs.Length; i++)
        {
            RegisterObject(pooledPrefabs[i].Name, pooledPrefabs[i].Prefab, pooledPrefabs[i].Lenght);
        }
    }

    /// <summary>
    /// Add a new pooled prefab
    /// </summary>
    /// <param name="poolName">Identifier of this pool</param>
    /// <param name="prefab"></param>
    /// <param name="count"></param>
    public void RegisterObject(string poolName, GameObject prefab, int count)
    {
        if(prefab == null)
        {
            Debug.LogWarning("Can't pooled the prefab for: " + poolName + " because the prefab has not been assigned.");
            return;
        }

        PoolObject p = new PoolObject();
        p.Name = poolName;
        p.Prefab = prefab;
        GameObject g = null;
        p.PoolList = new List<GameObject>();

        for (int i = 0; i < count; i++)
        {
            g = Instantiate(prefab) as GameObject;
            p.PoolList.Add(g);
            g.transform.parent = transform;
            g.SetActive(false);
        }
        m_poolList.Add(p);
    }

    /// <summary>
    /// Instantiate a pooled prefab
    /// use this instead of GameObject.Instantiate(...)
    /// </summary>
    /// <returns></returns>
    public GameObject Instantiate(string objectName, Vector3 position, Quaternion rotation)
    {
        PoolObject p = m_poolList.Find(x => x.Name == objectName);
        if(p != null)
        {
            GameObject g = p.GetCurrent;
            if(g == null)//in case a pool object get destroyed, replace it 
            {
                g = Instantiate(p.Prefab) as GameObject;
                p.ReplaceCurrent(g);
                g.transform.parent = transform;
            }
            g.transform.position = position;
            g.transform.rotation = rotation;
            g.SetActive(true);
            p.SetNext();
            return g;
        }
        else
        {
            Debug.LogError(string.Format("Object {0} has not been register for pooling.", objectName));
            return null;
        }
    }

    [System.Serializable]
    public class PoolObject
    {
        public string Name;
        public GameObject Prefab;
        public List<GameObject> PoolList = new List<GameObject>();
        public int CurrentPool;

        public GameObject GetCurrent
        {
            get
            {
                return PoolList[CurrentPool];
            }
        }

        public void SetNext() { CurrentPool = (CurrentPool + 1) % PoolList.Count; }

        public void ReplaceCurrent(GameObject g)
        {
            PoolList[CurrentPool] = g;
        }
    } 

    [System.Serializable]
    public class PreRegister
    {
        public string Name;
        public GameObject Prefab;
        public int Lenght;
    }

    private static bl_ObjectPooling _instance;
    public static bl_ObjectPooling Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_ObjectPooling>(); }
            return _instance;
        }
    }
}