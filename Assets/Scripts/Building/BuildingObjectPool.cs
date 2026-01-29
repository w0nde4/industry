using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingObjectPool : MonoBehaviour
{
    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly Dictionary<GameObject, GameObject> _activeObjects = new Dictionary<GameObject, GameObject>();
    
    private BuildingSettings _settings;
    private Transform _poolRoot;
    
    public void Initialize(BuildingSettings buildingSettings)
    {
        _settings = buildingSettings;
        _poolRoot = new GameObject("PreviewPool").transform;
        _poolRoot.SetParent(transform);
    }
    
    public GameObject GetPreview(GameObject prefab)
    {
        if (!_pools.ContainsKey(prefab))
        {
            _pools[prefab] = new Queue<GameObject>();
        }
    
        var pool = _pools[prefab];
        GameObject preview = null;
    
        while (pool.Count > 0)
        {
            preview = pool.Dequeue();
        
            if (preview != null)
            {
                preview.SetActive(true);
                preview.transform.SetParent(null);
                break;
            }
        
            preview = null;
        }
    
        if (preview == null)
        {
            preview = Instantiate(prefab, _poolRoot);
            preview.name = $"{prefab.name}_Preview_{GetInstanceID()}";

            DisableColliders(preview);
            
            if (_settings.ghostMaterial != null)
            {
                ApplyGhostMaterial(preview);
            }
            
            preview.transform.SetParent(null);
        }
    
        _activeObjects[preview] = prefab;
        return preview;
    }
    
    public void ReturnPreview(GameObject preview)
    {
        if (preview == null)
            return;

        if (!_activeObjects.TryGetValue(preview, out var prefab)) return;
        
        preview.SetActive(false);
        preview.transform.SetParent(_poolRoot);
        
        if (_pools[prefab].Count < _settings.maxPoolSize)
        {
            _pools[prefab].Enqueue(preview);
        }
        else
        {
            Destroy(preview);
        }
        
        _activeObjects.Remove(preview);
    }

    private void DisableColliders(GameObject obj)
    {
        var colliders = obj.GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        
        var colliders3D = obj.GetComponentsInChildren<Collider>();
        foreach (var col in colliders3D)
        {
            col.enabled = false;
        }
    }
    
    private void ApplyGhostMaterial(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var ren in renderers)
        {
            var mats = new Material[ren.materials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = _settings.ghostMaterial;
            }
            ren.materials = mats;
        }
    }

    private void OnDestroy()
    {
        ClearAll();
    }

    public void ClearAll()
    {
        var activeToDestroy = new List<GameObject>(_activeObjects.Keys);
        foreach (var preview in activeToDestroy
                     .Where(preview => preview != null))
        {
            DestroyImmediate(preview);
        }
        _activeObjects.Clear();
    
        foreach (var kvp in _pools)
        {
            while (kvp.Value.Count > 0)
            {
                var obj = kvp.Value.Dequeue();
                if (obj != null) DestroyImmediate(obj);
            }
        }
        _pools.Clear();
    }
}