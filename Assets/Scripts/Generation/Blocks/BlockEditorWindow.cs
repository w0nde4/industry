#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class BlockEditorWindow : EditorWindow
{
    private BlockData _currentBlock;
    private GameObject _blockPrefab;
    
    private Vector2 _scrollPos;
    private int _selectedCellX = -1;
    private int _selectedCellY = -1;
    
    private Sprite _paintSprite;
    private TerrainType _paintTerrainType = TerrainType.Grass;
    private CellModifiers _paintModifiers = new CellModifiers();
    
    private bool _showGrid = true;
    private float _cellDisplaySize = 32f;
    private readonly Color _gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    
    private enum EditorMode
    {
        Paint,
        Erase,
        Doors
    }
    
    private EditorMode _mode = EditorMode.Paint;
    
    [MenuItem("Tools/Level Generation/Block Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<BlockEditorWindow>("Block Editor");
        window.minSize = new Vector2(800, 600);
    }
    
    private void OnEnable()
    {
        _paintModifiers = new CellModifiers();
    }
    
    private void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        
        DrawHeader();
        
        EditorGUILayout.Space(10);
        
        if (_currentBlock != null)
        {
            DrawToolbar();
            
            EditorGUILayout.Space(10);
            
            DrawGrid();
            
            EditorGUILayout.Space(10);
            
            DrawSelectedCellInfo();
            
            EditorGUILayout.Space(10);
            
            DrawBlockActions();
        }
        else
        {
            DrawNoBlockSelected();
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Block Editor", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        _currentBlock = (BlockData)EditorGUILayout.ObjectField(
            "Current Block Data",
            _currentBlock,
            typeof(BlockData),
            false
        );
        
        if (GUILayout.Button("Create New Block", GUILayout.Width(150)))
        {
            CreateNewBlockData();
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (_currentBlock != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Block: {_currentBlock.BlockName} ({_currentBlock.BlockType})", EditorStyles.helpBox);
            EditorGUILayout.EndHorizontal();
        }
    }
    
    private void DrawToolbar()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Toggle(_mode == EditorMode.Paint, "Paint", EditorStyles.miniButtonLeft))
            _mode = EditorMode.Paint;
        
        if (GUILayout.Toggle(_mode == EditorMode.Erase, "Erase", EditorStyles.miniButtonMid))
            _mode = EditorMode.Erase;
        
        if (GUILayout.Toggle(_mode == EditorMode.Doors, "Doors", EditorStyles.miniButtonRight))
            _mode = EditorMode.Doors;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        if (_mode == EditorMode.Paint)
        {
            DrawPaintTools();
        }
        else if (_mode == EditorMode.Doors)
        {
            DrawDoorTools();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawPaintTools()
    {
        EditorGUILayout.LabelField("Paint Settings", EditorStyles.miniBoldLabel);
        
        _paintSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", _paintSprite, typeof(Sprite), false);
        _paintTerrainType = (TerrainType)EditorGUILayout.EnumPopup("Terrain Type", _paintTerrainType);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Cell Modifiers", EditorStyles.miniBoldLabel);
        
        _paintModifiers.isSpawnable = EditorGUILayout.Toggle("Is Spawnable", _paintModifiers.isSpawnable);
        _paintModifiers.isWalkable = EditorGUILayout.Toggle("Is Walkable", _paintModifiers.isWalkable);
        _paintModifiers.productionBonus = EditorGUILayout.FloatField("Production Bonus", _paintModifiers.productionBonus);
        _paintModifiers.biomeType = EditorGUILayout.TextField("Biome Type", _paintModifiers.biomeType);
        
        if (GUILayout.Button("Apply Terrain Type Defaults"))
        {
            ApplyTerrainDefaults();
        }
    }
    
    private void DrawDoorTools()
    {
        EditorGUILayout.LabelField("Door Management", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Door North")) AddDoor(DoorSide.North);
        if (GUILayout.Button("Add Door East")) AddDoor(DoorSide.East);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Door South")) AddDoor(DoorSide.South);
        if (GUILayout.Button("Add Door West")) AddDoor(DoorSide.West);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        if (_currentBlock.Doors.Count > 0)
        {
            EditorGUILayout.LabelField("Current Doors:", EditorStyles.miniBoldLabel);
            
            for (int i = _currentBlock.Doors.Count - 1; i >= 0; i--)
            {
                var door = _currentBlock.Doors[i];
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{door.Side} at {door.Position} (width: {door.Width})");
                
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    RemoveDoor(i);
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
    }
    
    private void DrawGrid()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
        _showGrid = EditorGUILayout.Toggle("Show Grid", _showGrid);
        _cellDisplaySize = EditorGUILayout.Slider("Cell Size", _cellDisplaySize, 16f, 64f);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        var blockSize = _currentBlock.BlockSize;
        var gridRect = GUILayoutUtility.GetRect(
            blockSize * _cellDisplaySize,
            blockSize * _cellDisplaySize,
            GUILayout.ExpandWidth(false),
            GUILayout.ExpandHeight(false)
        );
        
        DrawGridBackground(gridRect, blockSize);
        
        HandleGridInput(gridRect, blockSize);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawGridBackground(Rect gridRect, int blockSize)
    {
        // Фон
        EditorGUI.DrawRect(gridRect, new Color(0.2f, 0.2f, 0.2f));
        
        // Клетки
        for (int x = 0; x < blockSize; x++)
        {
            for (int y = 0; y < blockSize; y++)
            {
                var cellData = _currentBlock.GetCell(x, y);
                if (cellData == null)
                    continue;
                
                var cellRect = new Rect(
                    gridRect.x + x * _cellDisplaySize,
                    gridRect.y + (blockSize - 1 - y) * _cellDisplaySize,
                    _cellDisplaySize,
                    _cellDisplaySize
                );
                
                if (cellData.Sprite != null)
                {
                    GUI.DrawTexture(cellRect, cellData.Sprite.texture, ScaleMode.ScaleToFit);
                }
                else
                {
                    EditorGUI.DrawRect(cellRect, GetTerrainColor(cellData.Terrain));
                }
                
                if (x == _selectedCellX && y == _selectedCellY)
                {
                    EditorGUI.DrawRect(cellRect, new Color(1f, 1f, 0f, 0.3f));
                }
                
                if (_showGrid)
                {
                    Handles.BeginGUI();
                    Handles.color = _gridColor;
                    Handles.DrawLine(
                        new Vector3(cellRect.x, cellRect.y),
                        new Vector3(cellRect.x + cellRect.width, cellRect.y)
                    );
                    Handles.DrawLine(
                        new Vector3(cellRect.x, cellRect.y),
                        new Vector3(cellRect.x, cellRect.y + cellRect.height)
                    );
                    Handles.EndGUI();
                }
            }
        }
        
        DrawDoors(gridRect, blockSize);
    }
    
    private void DrawDoors(Rect gridRect, int blockSize)
    {
        Handles.BeginGUI();
        Handles.color = Color.green;
        
        foreach (var door in _currentBlock.Doors)
        {
            var doorCells = door.GetDoorCells(blockSize);
            
            foreach (var cell in doorCells)
            {
                var cellRect = new Rect(
                    gridRect.x + cell.x * _cellDisplaySize,
                    gridRect.y + (blockSize - 1 - cell.y) * _cellDisplaySize,
                    _cellDisplaySize,
                    _cellDisplaySize
                );
                
                Handles.DrawSolidRectangleWithOutline(cellRect, new Color(0f, 1f, 0f, 0.3f), Color.green);
            }
        }
        
        Handles.EndGUI();
    }
    
    private void HandleGridInput(Rect gridRect, int blockSize)
    {
        var e = Event.current;
        
        if (e.type == EventType.MouseDown && gridRect.Contains(e.mousePosition))
        {
            var localPos = e.mousePosition - gridRect.position;
            var cellX = Mathf.FloorToInt(localPos.x / _cellDisplaySize);
            var cellY = blockSize - 1 - Mathf.FloorToInt(localPos.y / _cellDisplaySize);
            
            if (cellX >= 0 && cellX < blockSize && cellY >= 0 && cellY < blockSize)
            {
                OnCellClicked(cellX, cellY);
                e.Use();
            }
        }
    }
    
    private void OnCellClicked(int x, int y)
    {
        _selectedCellX = x;
        _selectedCellY = y;
        
        if (_mode == EditorMode.Paint)
        {
            PaintCell(x, y);
        }
        else if (_mode == EditorMode.Erase)
        {
            EraseCell(x, y);
        }
        
        EditorUtility.SetDirty(_currentBlock);
        Repaint();
    }
    
    private void PaintCell(int x, int y)
    {
        var cellData = new CellData(_paintSprite, _paintTerrainType, _paintModifiers);
        _currentBlock.SetCell(x, y, cellData);
    }
    
    private void EraseCell(int x, int y)
    {
        _currentBlock.SetCell(x, y, new CellData());
    }
    
    private void DrawSelectedCellInfo()
    {
        if (_selectedCellX < 0 || _selectedCellY < 0)
            return;
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"Selected Cell: ({_selectedCellX}, {_selectedCellY})", EditorStyles.boldLabel);
        
        var cellData = _currentBlock.GetCell(_selectedCellX, _selectedCellY);
        if (cellData != null)
        {
            EditorGUILayout.LabelField($"Terrain: {cellData.Terrain}");
            EditorGUILayout.LabelField($"Spawnable: {cellData.Modifiers.isSpawnable}");
            EditorGUILayout.LabelField($"Walkable: {cellData.Modifiers.isWalkable}");
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawBlockActions()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Save Block Data"))
        {
            EditorUtility.SetDirty(_currentBlock);
            AssetDatabase.SaveAssets();
            Debug.Log($"[BlockEditor] Saved {_currentBlock.BlockName}");
        }
        
        if (GUILayout.Button("Create Prefab from Block"))
        {
            CreatePrefabFromBlock();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Validate Block"))
        {
            _currentBlock.ValidateDoors();
        }
        
        if (GUILayout.Button("Clear All Cells"))
        {
            if (EditorUtility.DisplayDialog("Clear All Cells", "Are you sure?", "Yes", "No"))
            {
                _currentBlock.InitializeCells();
                EditorUtility.SetDirty(_currentBlock);
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawNoBlockSelected()
    {
        EditorGUILayout.HelpBox("No BlockData selected. Create a new one or select an existing BlockData asset.", MessageType.Info);
    }
    
    private void CreateNewBlockData()
    {
        var path = EditorUtility.SaveFilePanelInProject(
            "Create Block Data",
            "NewBlock",
            "asset",
            "Enter a name for the new block"
        );
        
        if (string.IsNullOrEmpty(path))
            return;
        
        var newBlock = CreateInstance<BlockData>();
        AssetDatabase.CreateAsset(newBlock, path);
        AssetDatabase.SaveAssets();
        
        _currentBlock = newBlock;
        _currentBlock.InitializeCells();
        
        EditorUtility.SetDirty(_currentBlock);
        
        Debug.Log($"[BlockEditor] Created new BlockData at {path}");
    }
    
    private void CreatePrefabFromBlock()
    {
        if (_currentBlock == null)
            return;
        
        var path = EditorUtility.SaveFilePanelInProject(
            "Create Block Prefab",
            _currentBlock.BlockName,
            "prefab",
            "Enter a name for the prefab"
        );
        
        if (string.IsNullOrEmpty(path))
            return;
        
        var blockObj = new GameObject(_currentBlock.BlockName);
        var block = blockObj.AddComponent<Block>();
        
        var serializedBlock = new SerializedObject(block);
        serializedBlock.FindProperty("blockData").objectReferenceValue = _currentBlock;
        serializedBlock.ApplyModifiedProperties();
        
        block.GenerateVisuals();
        
        PrefabUtility.SaveAsPrefabAsset(blockObj, path);
        DestroyImmediate(blockObj);
        
        Debug.Log($"[BlockEditor] Created prefab at {path}");
    }
    
    private void AddDoor(DoorSide side)
    {
        var door = new BlockDoor(side, 7, 2); // Центр стороны, ширина 2
        _currentBlock.Doors.Add(door);
        EditorUtility.SetDirty(_currentBlock);
        Repaint();
    }
    
    private void RemoveDoor(int index)
    {
        _currentBlock.Doors.RemoveAt(index);
        EditorUtility.SetDirty(_currentBlock);
        Repaint();
    }
    
    private void ApplyTerrainDefaults()
    {
        var tempCell = new CellData();
        tempCell.SetTerrainType(_paintTerrainType);
        _paintModifiers = tempCell.Modifiers.Clone();
        Repaint();
    }
    
    private Color GetTerrainColor(TerrainType type)
    {
        return type switch
        {
            TerrainType.Grass => new Color(0.2f, 0.8f, 0.2f),
            TerrainType.Stone => new Color(0.5f, 0.5f, 0.5f),
            TerrainType.Sand => new Color(0.9f, 0.8f, 0.5f),
            TerrainType.Water => new Color(0.2f, 0.4f, 0.8f),
            TerrainType.Obstacle => new Color(0.3f, 0.2f, 0.1f),
            TerrainType.Destructible => new Color(0.8f, 0.4f, 0.2f),
            _ => Color.white
        };
    }
}
#endif