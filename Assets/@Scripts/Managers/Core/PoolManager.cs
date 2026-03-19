using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
    private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();

    public void Initialize()
    {
        if (IsInitialized) return;
        IsInitialized = true;
    }

    public void CreatePool(GameObject prefab, int initialSize)
    {
        if (prefab == null || initialSize <= 0)
            return;

        Queue<GameObject> pool = GetOrCreatePool(prefab);

        for (int i = 0; i < initialSize; i++)
        {
            GameObject instance = CreateNewInstance(prefab);
            ReturnInternal(prefab, instance);
        }
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogError("[PoolManager] Prefab is null.");
            return null;
        }

        Queue<GameObject> pool = GetOrCreatePool(prefab);

        GameObject instance;

        if (pool.Count > 0)
        {
            instance = pool.Dequeue();
        }
        else
        {
            instance = CreateNewInstance(prefab);
        }

        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);

        return instance;
    }

    public void Return(GameObject instance)
    {
        if (instance == null)
            return;

        if (!_instanceToPrefab.TryGetValue(instance, out GameObject prefab))
        {
            Debug.LogWarning("[PoolManager] Returned object was not created by PoolManager. Destroying it.");
            Destroy(instance);
            return;
        }

        ReturnInternal(prefab, instance);
    }

    private GameObject CreateNewInstance(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab);
        _instanceToPrefab[instance] = prefab;
        return instance;
    }

    private void ReturnInternal(GameObject prefab, GameObject instance)
    {
        instance.SetActive(false);

        Queue<GameObject> pool = GetOrCreatePool(prefab);
        pool.Enqueue(instance);
    }

    // 풀을 반환하거나 없으면 생성해서 반환
    private Queue<GameObject> GetOrCreatePool(GameObject prefab)
    {
        if (!_pools.TryGetValue(prefab, out var pool))
        {
            pool = new Queue<GameObject>();
            _pools[prefab] = pool;
        }
        return pool;
    }
}