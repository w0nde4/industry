#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class BlockEditorWindow : EditorWindow
{
    private BlockData _currentBlock;
    
    private Vector2 _scrollPos;
    private int _selectedCellX = -1;
    private int _selectedCellY = -1;
    
    private Sprite _paintSprite;
    private TerrainType _paintTerrainType = TerrainType.Grass;
    private CellModifiers _paintModifiers = new CellModifiers();
    
    private bool _showGrid = true;
    private const float CellDisplaySize = 32f; // Фиксированный размер
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
        _showGrid = EditorGUILayout.Toggle("Show Grid Lines", _showGrid);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        var blockSize = _currentBlock.BlockSize;
        var gridRect = GUILayoutUtility.GetRect(
            blockSize * CellDisplaySize,
            blockSize * CellDisplaySize,
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
        
        // Рисуем клетки
        for (int x = 0; x < blockSize; x++)
        {
            for (int y = 0; y < blockSize; y++)
            {
                var cellData = _currentBlock.GetCell(x, y);
                if (cellData == null)
                    continue;
                
                var cellRect = new Rect(
                    gridRect.x + x * CellDisplaySize,
                    gridRect.y + (blockSize - 1 - y) * CellDisplaySize, // Инвертируем Y
                    CellDisplaySize,
                    CellDisplaySize
                );
                
                // Отрисовка спрайта или цвета
                if (cellData.Sprite != null)
                {
                    DrawSprite(cellRect, cellData.Sprite);
                }
                else
                {
                    // Рисуем только если не пустая клетка
                    var color = GetTerrainColor(cellData.Terrain);
                    if (color.a > 0) // Если цвет не прозрачный
                    {
                        EditorGUI.DrawRect(cellRect, color);
                    }
                }
                
                // Выделение выбранной клетки
                if (x == _selectedCellX && y == _selectedCellY)
                {
                    EditorGUI.DrawRect(cellRect, new Color(1f, 1f, 0f, 0.3f));
                }
                
                // Сетка
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
        
        // Рисуем двери
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
                    gridRect.x + cell.x * CellDisplaySize,
                    gridRect.y + (blockSize - 1 - cell.y) * CellDisplaySize,
                    CellDisplaySize,
                    CellDisplaySize
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
            var cellX = Mathf.FloorToInt(localPos.x / CellDisplaySize);
            var cellY = blockSize - 1 - Mathf.FloorToInt(localPos.y / CellDisplaySize);
            
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
        // Создаём пустую клетку с прозрачным цветом
        var emptyCellData = new CellData(null, TerrainType.Grass, new CellModifiers());
        _currentBlock.SetCell(x, y, emptyCellData);
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
            EditorGUILayout.LabelField($"Sprite: {(cellData.Sprite != null ? cellData.Sprite.name : "None")}");
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
                Repaint();
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
        
        Repaint();
    }
    
    private void CreatePrefabFromBlock()
    {
        if (_currentBlock == null)
        {
            Debug.LogError("[BlockEditor] No block data selected!");
            return;
        }
        
        // Проверяем что у блока есть хотя бы одна клетка со спрайтом
        bool hasSprites = false;
        for (int x = 0; x < _currentBlock.BlockSize; x++)
        {
            for (int y = 0; y < _currentBlock.BlockSize; y++)
            {
                var cell = _currentBlock.GetCell(x, y);
                if (cell != null && cell.Sprite != null)
                {
                    hasSprites = true;
                    break;
                }
            }
            if (hasSprites) break;
        }
        
        if (!hasSprites)
        {
            if (!EditorUtility.DisplayDialog(
                "No Sprites", 
                "Block has no cells with sprites. Create empty prefab?", 
                "Yes", "No"))
            {
                return;
            }
        }
        
        var path = EditorUtility.SaveFilePanelInProject(
            "Create Block Prefab",
            _currentBlock.BlockName,
            "prefab",
            "Enter a name for the prefab"
        );
        
        if (string.IsNullOrEmpty(path))
            return;
        
        // Создаём временный объект в сцене
        var blockObj = new GameObject(_currentBlock.BlockName);
        blockObj.transform.position = Vector3.zero;
        
        // Добавляем компонент Block
        var block = blockObj.AddComponent<Block>();
        
        // Устанавливаем BlockData через SerializedObject (для приватного поля)
        var serializedBlock = new SerializedObject(block);
        serializedBlock.FindProperty("blockData").objectReferenceValue = _currentBlock;
        serializedBlock.ApplyModifiedProperties();
        
        // ВАЖНО: Генерируем визуалы ДО сохранения в префаб
        // Это создаст все дочерние GameObject'ы с SpriteRenderer'ами
        block.GenerateVisuals();
        
        // Сохраняем как префаб (со всеми дочерними объектами)
        var prefab = PrefabUtility.SaveAsPrefabAsset(blockObj, path);
        
        if (prefab != null)
        {
            // Подсчитываем количество клеток
            int cellCount = 0;
            var cellsContainer = blockObj.transform.Find("Cells");
            if (cellsContainer != null)
            {
                cellCount = cellsContainer.childCount;
            }
            
            Debug.Log($"[BlockEditor] Created prefab at {path} with {cellCount} cell visuals");
            
            // Выделяем созданный префаб в Project
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }
        else
        {
            Debug.LogError("[BlockEditor] Failed to create prefab!");
        }
        
        // Удаляем временный объект из сцены
        DestroyImmediate(blockObj);
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
            _ => new Color(0, 0, 0, 0) // Прозрачный для пустых клеток
        };
    }
    
    /// <summary>
    /// Корректная отрисовка спрайта с учётом UV координат
    /// </summary>
    private void DrawSprite(Rect rect, Sprite sprite)
    {
        if (sprite == null || sprite.texture == null)
            return;

        var texture = sprite.texture;
        var spriteRect = sprite.textureRect;
        
        // Нормализуем UV координаты
        var uvRect = new Rect(
            spriteRect.x / texture.width,
            spriteRect.y / texture.height,
            spriteRect.width / texture.width,
            spriteRect.height / texture.height
        );
        
        // Отрисовываем с правильными UV
        GUI.DrawTextureWithTexCoords(rect, texture, uvRect);
    }
}
#endif