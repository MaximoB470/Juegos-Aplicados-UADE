using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator : MonoBehaviour
{
    private Dictionary<string, MonoBehaviour> servicesByName = new();

    private static ServiceLocator _instance;
    public static ServiceLocator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ServiceLocator>();
            }

            if (_instance == null)
            {
                var newGO = new GameObject("ServiceLocator");
                _instance = newGO.AddComponent<ServiceLocator>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public MonoBehaviour GetService(string serviceName)
    {
        servicesByName.TryGetValue(serviceName, out var service);
        return service;
    }

    public void SetService(string serviceName, MonoBehaviour value)
    {
        servicesByName[serviceName] = value;
    }

    public bool IsInitialized()
    {
        return _instance != null;
    }
}