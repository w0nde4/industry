using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BuildingPlacer placer;
    [SerializeField] private Camera mainCamera;
    
    [Header("Building Data")]
    [SerializeField] private BuildingData[] buildingHotkeys = new BuildingData[6]; // Q W E R T Y
    [SerializeField] private BuildingData[] conveyorHotkeys = new BuildingData[6]; // Q W E R T Y
    
    private PlayerInput _playerInput;
    private InputAction _mousePositionAction;
    private InputAction _leftClickAction;
    private InputAction _rightClickAction;
    private InputAction _mouseScrollAction;
    private InputAction _buildModeAction;
    private InputAction _conveyorModeAction;
    private InputAction _demolishModeAction;
    private InputAction[] _buildingActions = new InputAction[6];
    
    private enum BuildMode
    {
        None,
        Build,
        Conveyor,
        Demolish
    }
    
    private BuildMode _currentMode = BuildMode.None;
    
    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput == null)
        {
            _playerInput = gameObject.AddComponent<PlayerInput>();
        }
    }

    private void Start()
    {
        SetupInput();
    }

    private void OnDisable()
    {
        UnsubscribeInput();
    }

    private void OnDestroy()
    {
        UnsubscribeInput();
    }

    private void UnsubscribeInput()
    {
        
        if (_leftClickAction != null)
            _leftClickAction.performed -= OnLeftClick;
    
        if (_rightClickAction != null)
            _rightClickAction.performed -= OnRightClick;
    
        if (_mouseScrollAction != null)
            _mouseScrollAction.performed -= OnMouseScroll;
        
        if (_buildModeAction != null)
            _buildModeAction.performed -= OnBuildModePressed;
    
        if (_demolishModeAction != null)
            _demolishModeAction.performed -= OnDemolishModePressed;
    
        // Отписываемся от зданий
        for (int i = 0; i < _buildingActions.Length; i++)
        {
            if (_buildingActions[i] == null) continue;
            
            var index = i;
            _buildingActions[i].performed -= ctx => OnBuildingSelected(index);
        }
    }

    private void SetupInput()
    {
        _mousePositionAction = _playerInput.actions.FindAction("MousePosition");
        _leftClickAction = _playerInput.actions.FindAction("LeftClick");
        _rightClickAction = _playerInput.actions.FindAction("RightClick");
        _mouseScrollAction = _playerInput.actions.FindAction("MouseScroll");
    
        if (_leftClickAction != null)
            _leftClickAction.performed += OnLeftClick;
    
        if (_rightClickAction != null)
            _rightClickAction.performed += OnRightClick;
    
        if (_mouseScrollAction != null)
            _mouseScrollAction.performed += OnMouseScroll;
    
        _buildModeAction = _playerInput.actions.FindAction("BuildMode");
        if (_buildModeAction != null)
            _buildModeAction.performed += OnBuildModePressed;
        
        _conveyorModeAction = _playerInput.actions.FindAction("ConveyorMode");
        if (_conveyorModeAction != null)
            _conveyorModeAction.performed += OnConveyorModePressed;

        _demolishModeAction = _playerInput.actions.FindAction("DemolishMode");
        if (_demolishModeAction != null)
            _demolishModeAction.performed += OnDemolishModePressed;

        for (int i = 0; i < 6; i++)
        {
            _buildingActions[i] = _playerInput.actions.FindAction($"Building{i + 1}");
            if (_buildingActions[i] == null) continue;
            
            var index = i; // Capture для замыкания
            _buildingActions[i].performed += ctx => OnBuildingSelected(index);
        }
    }

    private void Update()
    {
        if (_currentMode == BuildMode.None)
            return;
        
        var mouseWorldPos = GetMouseWorldPosition();
        placer.UpdatePreview(mouseWorldPos);
    }

    private void SetMode(BuildMode mode)
    {
        _currentMode = mode;

        switch (mode)
        {
            case BuildMode.Build:
                placer.StopBuildMode();
                Debug.Log("Build Mode: Select a building (Q/W/E/R/T/Y)");
                break;
            case BuildMode.Conveyor:
                placer.StopBuildMode();
                Debug.Log("Build Mode: Select a conveyor (Q/W/E/R/T/Y)");
                break;
            case BuildMode.Demolish:
                placer.StopBuildMode();
                Debug.Log("Demolish Mode: Click on building to demolish");
                break;
        }
    }

    private void SelectBuilding(int index)
    {
        BuildingData selectedData;

        switch (_currentMode)
        {
            case BuildMode.Build:
            {
                if (index < 0 || index >= buildingHotkeys.Length || buildingHotkeys[index] == null)
                {
                    Debug.LogWarning($"Building hotkey {index} not assigned!");
                    return;
                }
                selectedData = buildingHotkeys[index];
                break;
            }
            case BuildMode.Conveyor:
            {
                if (index < 0 || index >= conveyorHotkeys.Length || conveyorHotkeys[index] == null)
                {
                    Debug.LogWarning($"Conveyor hotkey {index} not assigned!");
                    return;
                }

                selectedData = conveyorHotkeys[index];
                break;
            }
            default:
                return;
        }
        
        placer.StartBuildMode(selectedData);
        Debug.Log($"Selected: {selectedData.buildingName}");
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (placer == null)
            return;
        
        var mouseWorldPos = GetMouseWorldPosition();
        
        switch (_currentMode)
        {
            case BuildMode.Build:
            case BuildMode.Conveyor:
                placer.UpdatePreview(mouseWorldPos);
                placer.TryPlaceBuilding();
                break;
            case BuildMode.Demolish:
                placer.TryDemolish(mouseWorldPos);
                break;
        }
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        if (placer.IsInMoveMode)
        {
            placer.CancelMove();
        }
        else if (_currentMode != BuildMode.None)
        {
            placer.StopBuildMode();
            _currentMode = BuildMode.None;
            Debug.Log("Build mode cancelled");
        }
    }

    private void OnMouseScroll(InputAction.CallbackContext context)
    {
        if (_currentMode != BuildMode.Build
            && _currentMode != BuildMode.Conveyor
            && !placer.IsInMoveMode)
            return;
        
        var scrollValue = context.ReadValue<float>();
        var direction = scrollValue > 0 ? 1 : -1;
        
        placer.RotatePreview(direction);
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (_mousePositionAction == null)
            return Vector3.zero;
        
        var screenPos = _mousePositionAction.ReadValue<Vector2>();
        var worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;
        
        return worldPos;
    }


    private void OnBuildModePressed(InputAction.CallbackContext ctx) => SetMode(BuildMode.Build);
    private void OnConveyorModePressed(InputAction.CallbackContext ctx) => SetMode(BuildMode.Conveyor);
    private void OnDemolishModePressed(InputAction.CallbackContext ctx) => SetMode(BuildMode.Demolish);
    private void OnBuildingSelected(int index) => SelectBuilding(index);
}