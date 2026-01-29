using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPreview : MonoBehaviour
{
    private BuildingData _currentData;
    private BuildingRotation _currentRotation = BuildingRotation.North;
    private GameObject _previewObject;
    private BuildingSettings _settings;
    private GridSystem _gridSystem;
    private BuildingObjectPool _objectPool;
    private Vector2Int _currentGridPosition;
    
    private readonly List<Renderer> _previewRenderers = new List<Renderer>();
    private bool _isValid = false;
    
    public bool IsActive { get; private set; }
    public BuildingData CurrentData => _currentData;
    public BuildingRotation CurrentRotation => _currentRotation;
    
    public void Initialize(BuildingSettings buildingSettings, GridSystem grid, BuildingObjectPool pool)
    {
        _settings = buildingSettings;
        _gridSystem = grid;
        _objectPool = pool;
    }
    
    public void StartPreview(BuildingData data)
    {
        if (IsActive)
        {
            StopPreview();
        }
        
        _currentData = data;
        _currentRotation = BuildingRotation.North;
        
        _previewObject = _objectPool.GetPreview(data.prefab);
        _previewRenderers.Clear();
        _previewRenderers.AddRange(_previewObject.GetComponentsInChildren<Renderer>());
        
        // Отключаем коллайдеры у превью
        var colliders = _previewObject.GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        
        IsActive = true;
    }
    
    public void StopPreview()
    {
        if (_previewObject != null)
        {
            _objectPool.ReturnPreview(_previewObject);
            _previewObject = null;
        }
        
        _previewRenderers.Clear();
        IsActive = false;
        _currentData = null;
        _currentGridPosition = Vector2Int.zero;
    }
    
    public void UpdatePreview(Vector3 worldPosition)
    {
        if (!IsActive || _currentData == null)
            return;
        
        _currentGridPosition = _gridSystem.WorldToGridPosition(worldPosition);
        var size = _currentData.GetRotatedSize(_currentRotation);
        
        // Проверяем доступность
        _isValid = _gridSystem.IsAreaAvailable(_currentGridPosition, size);
        
        // Позиционируем превью
        var snappedPos = _gridSystem.GetCenterPosition(_currentGridPosition, size);
        _previewObject.transform.position = snappedPos;
        
        // Применяем ротацию
        _previewObject.transform.rotation = Quaternion.Euler(0, 0, -(int)_currentRotation);
        
        // Обновляем цвет
        UpdatePreviewColor();
    }
    
    public void Rotate(int direction)
    {
        if (!IsActive || _currentData == null || !_currentData.canRotate)
            return;
        
        var rotationValue = (int)_currentRotation;
        rotationValue += direction * 90;

        rotationValue = rotationValue switch
        {
            >= 360 => 0,
            < 0 => 270,
            _ => rotationValue
        };

        _currentRotation = (BuildingRotation)rotationValue;
    }
    
    private void UpdatePreviewColor()
    {
        var targetColor = _isValid ? _settings.validPlacementColor : _settings.invalidPlacementColor;
        
        foreach (var ren in _previewRenderers)
        {
            ren.material.color = targetColor;
        }
    }
    
    public bool CanPlace()
    {
        return IsActive && _isValid;
    }
    
    public Vector2Int GetCurrentGridPosition()
    {
        return _currentGridPosition;
    }

    private void OnDestroy()
    {
        StopPreview();
    }
}