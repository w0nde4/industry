using System.Collections.Generic;
using UnityEngine;

public class PathfindingSystem : MonoBehaviour
{
    private GridSystem _grid;
    private Dictionary<Vector2Int, Dictionary<Vector2Int, List<Vector2Int>>> _pathCache;
    
    private int _totalCachedPaths = 0;
    
    private const int MAX_CACHE_SIZE = 100;
    
    private static readonly Vector2Int[] Directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0)
    };
    
    private void Awake()
    {
        if(GridService.Instance != null)
        {
            _grid = GridService.Instance.Grid;
        }

        _pathCache = new Dictionary<Vector2Int, Dictionary<Vector2Int, List<Vector2Int>>>();
    }
    
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        if (_grid == null)
        {
            Debug.LogError("[PathfindingSystem] Grid is null!");
            return null;
        }
        
        if (_pathCache.TryGetValue(start, out var endCache))
        {
            if (endCache.TryGetValue(end, out var cachedPath))
            {
                return new List<Vector2Int>(cachedPath);
            }
        }
        
        var path = AStar(start, end);
        
        if (path != null && path.Count > 0)
        {
            CachePath(start, end, path);
        }
        
        return path;
    }
    
    private List<Vector2Int> AStar(Vector2Int start, Vector2Int end)
    {
        var openSet = new MinHeap<AStarNode>();
        var closedSet = new HashSet<Vector2Int>();
        var allNodes = new Dictionary<Vector2Int, AStarNode>();
        
        var startNode = new AStarNode(start, null, 0, GetHeuristic(start, end));
        openSet.Add(startNode);
        allNodes[start] = startNode;
        
        while (openSet.Count > 0)
        {
            var current = openSet.RemoveMin();
            
            if (current.Position == end)
            {
                return ReconstructPath(current);
            }
            
            if (closedSet.Contains(current.Position)) continue;
            closedSet.Add(current.Position);
            
            foreach (var neighborPos in GetWalkableNeighbors(current.Position))
            {
                if (closedSet.Contains(neighborPos)) continue;
                
                var gCost = current.GCost + 1;
                var hCost = GetHeuristic(neighborPos, end);
                
                if (!allNodes.TryGetValue(neighborPos, out var neighborNode))
                {
                    neighborNode = new AStarNode(neighborPos, current, gCost, hCost);
                    allNodes[neighborPos] = neighborNode;
                    openSet.Add(neighborNode);
                }
                else if (gCost < neighborNode.GCost)
                {
                    neighborNode.Parent = current;
                    neighborNode.GCost = gCost;
                    openSet.Add(neighborNode);
                }
            }
        }
        
        return null;
    }
    
    private List<Vector2Int> GetWalkableNeighbors(Vector2Int position)
    {
        var neighbors = new List<Vector2Int>(4);
        
        foreach (var dir in Directions)
        {
            var neighborPos = position + dir;
            var cell = _grid.GetCell(neighborPos);
            
            if (cell != null && cell.Modifiers.isWalkable)
            {
                neighbors.Add(neighborPos);
            }
        }
        
        return neighbors;
    }
    
    private int GetHeuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
    
    private List<Vector2Int> ReconstructPath(AStarNode endNode)
    {
        var path = new List<Vector2Int>();
        var current = endNode;
        
        while (current != null)
        {
            path.Add(current.Position);
            current = current.Parent;
        }
        
        path.Reverse();
        return path;
    }
    
    private void CachePath(Vector2Int start, Vector2Int end, List<Vector2Int> path)
    {
        if (_totalCachedPaths >= MAX_CACHE_SIZE)
        {
            _pathCache.Clear();
            _totalCachedPaths = 0;
        }
        
        if (!_pathCache.ContainsKey(start))
        {
            _pathCache[start] = new Dictionary<Vector2Int, List<Vector2Int>>();
        }
        
        if (!_pathCache[start].ContainsKey(end))
            _totalCachedPaths++;
        
        _pathCache[start][end] = new List<Vector2Int>(path);
    }
    
    public void InvalidateCache()
    {
        _pathCache.Clear();
        Debug.Log("[PathfindingSystem] Path cache invalidated");
    }
    
    private class AStarNode : System.IComparable<AStarNode>
    {
        public Vector2Int Position;
        public AStarNode Parent;
        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;
        
        public AStarNode(Vector2Int position, AStarNode parent, int gCost, int hCost)
        {
            Position = position;
            Parent = parent;
            GCost = gCost;
            HCost = hCost;
        }
        
        public int CompareTo(AStarNode other)
        {
            return FCost.CompareTo(other.FCost);
        }
    }
    
    public class MinHeap<T> where T : System.IComparable<T>
    {
        private List<T> _items = new List<T>();
    
        public int Count => _items.Count;
    
        public void Add(T item)
        {
            _items.Add(item);
            HeapifyUp(_items.Count - 1);
        }
    
        public T RemoveMin()
        {
            if (_items.Count == 0)
                throw new System.InvalidOperationException("Heap is empty");
        
            var min = _items[0];
            _items[0] = _items[_items.Count - 1];
            _items.RemoveAt(_items.Count - 1);
        
            if (_items.Count > 0)
                HeapifyDown(0);
        
            return min;
        }
    
        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                var parent = (index - 1) / 2;
            
                if (_items[index].CompareTo(_items[parent]) >= 0)
                    break;
            
                Swap(index, parent);
                index = parent;
            }
        }
    
        private void HeapifyDown(int index)
        {
            while (true)
            {
                var left = 2 * index + 1;
                var right = 2 * index + 2;
                var smallest = index;
            
                if (left < _items.Count && _items[left].CompareTo(_items[smallest]) < 0)
                    smallest = left;
            
                if (right < _items.Count && _items[right].CompareTo(_items[smallest]) < 0)
                    smallest = right;
            
                if (smallest == index)
                    break;
            
                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int i, int j)
        {
            (_items[i], _items[j]) = (_items[j], _items[i]);
        }
    }
}