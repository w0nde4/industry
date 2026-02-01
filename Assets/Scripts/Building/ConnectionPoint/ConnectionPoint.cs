using System;
using UnityEngine;

public class ConnectionPoint : MonoBehaviour
{
    [SerializeField] private ConnectionType type;
    
    private PlacedBuilding _owner;
    private Vector3 _worldPosition;
    
    public ConnectionType Type => type;
    public PlacedBuilding Owner => _owner;
    public Vector3 WorldPosition => _worldPosition;
    
    public event Action<ConnectionPoint, ResourceInstance> OnResourceReceived;
    public event Action<ConnectionPoint, ResourceInstance> OnResourceSent;
    
    public void Initialize(PlacedBuilding owner)
    {
        _owner = owner;
        UpdateWorldPosition();
    }
    
    public void UpdateWorldPosition()
    {
        if (_owner == null)
        {
            _worldPosition = transform.position;
            return;
        }
        
        var localPos = transform.localPosition;
        var rotation = _owner.Rotation;
        var rotatedOffset = RotateOffset(localPos, rotation);
        
        _worldPosition = _owner.transform.position + new Vector3(rotatedOffset.x, rotatedOffset.y, 0);
    }
    
    private Vector2 RotateOffset(Vector2 offset, BuildingRotation rotation)
    {
        var angle = -(int)rotation;
        var rad = angle * Mathf.Deg2Rad;
        
        var cos = Mathf.Cos(rad);
        var sin = Mathf.Sin(rad);
        
        return new Vector2(
            offset.x * cos - offset.y * sin,
            offset.x * sin + offset.y * cos
        );
    }
    
    public bool CanReceive(ResourceInstance resource)
    {
        if (type != ConnectionType.Input) return false;
        if (resource == null) return false;
        
        return true;
    }
    
    public void ReceiveResource(ResourceInstance resource)
    {
        if (!CanReceive(resource))
        {
            Debug.LogWarning($"[ConnectionPoint] Cannot receive resource at {_owner?.Data?.buildingName}");
            return;
        }
        
        OnResourceReceived?.Invoke(this, resource);
    }
    
    public void SendResource(ResourceInstance resource)
    {
        if (type != ConnectionType.Output)
        {
            Debug.LogWarning($"[ConnectionPoint] Trying to send from Input point");
            return;
        }
        
        OnResourceSent?.Invoke(this, resource);
    }
    
    public Vector2Int GetGridPosition(GridSystem grid)
    {
        if (_owner == null || grid == null) return Vector2Int.zero;
        
        return grid.WorldToGridPosition(_worldPosition);
    }
    
    #region Editor Visualization
    
    private static ConnectionPointSettings _cachedSettings;

    private static ConnectionPointSettings GetSettings()
    {
        if (_cachedSettings == null)
        {
            _cachedSettings = Resources.Load<ConnectionPointSettings>("ConnectionPointSettings");
            
            #if UNITY_EDITOR
            if (_cachedSettings == null)
            {
                Debug.LogWarning("[ConnectionPoint] ConnectionPointSettings not found in Resources folder. Using default values.");
            }
            #endif
            
            if (_cachedSettings == null)
            {
                _cachedSettings = ScriptableObject.CreateInstance<ConnectionPointSettings>();
            }
        }
        
        return _cachedSettings;
    }
    private void OnDrawGizmos()
    {
        UpdateWorldPosition();
        
        var settings = GetSettings();
        Gizmos.color = type == ConnectionType.Input ? settings.inputColor : settings.outputColor;
        Gizmos.DrawWireSphere(_worldPosition, settings.gizmoSize);
        
        var direction = transform.up * settings.directionLineLength;
        Gizmos.DrawLine(_worldPosition, _worldPosition + direction);
    }
    
    private void OnDrawGizmosSelected()
    {
        UpdateWorldPosition();
        
        var settings = GetSettings();
        Gizmos.color = type == ConnectionType.Input ? settings.inputColor : settings.outputColor;
        Gizmos.DrawSphere(_worldPosition, settings.gizmoSize);
        
        #if UNITY_EDITOR
        var style = new GUIStyle
        {
            normal =
            {
                textColor = type == ConnectionType.Input ? settings.inputColor : settings.outputColor
            },
            fontSize = 12
        };
        UnityEditor.Handles.Label(_worldPosition + Vector3.up * 0.5f, type.ToString(), style);
        #endif
    }
    
    #endregion
}