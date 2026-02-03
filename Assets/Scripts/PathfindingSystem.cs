using System.Collections.Generic;
using UnityEngine;

public class PathfindingSystem : MonoBehaviour
{
    private GridSystem _grid;
    private Dictionary<Vector2Int, Dictionary<Vector2Int, List<Vector2Int>>> _pathCache;
    
    private const int MAX_CACHE_SIZE = 1000;
    
    private void Awake()
    {
        _grid = GridService.Instance.Grid;
        _pathCache = new Dictionary<Vector2Int, Dictionary<Vector2Int, List<Vector2Int>>>();
    }
    
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
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
        var openSet = new List<AStarNode>();
        var closedSet = new HashSet<Vector2Int>();
        var allNodes = new Dictionary<Vector2Int, AStarNode>();
        
        var startNode = new AStarNode(start, null, 0, GetHeuristic(start, end));
        openSet.Add(startNode);
        allNodes[start] = startNode;
        
        while (openSet.Count > 0)
        {
            var current = GetLowestFCost(openSet);
            
            if (current.Position == end)
            {
                return ReconstructPath(current);
            }
            
            openSet.Remove(current);
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
                }
            }
        }
        
        return null;
    }
    
    private List<Vector2Int> GetWalkableNeighbors(Vector2Int position)
    {
        var neighbors = new List<Vector2Int>(4);
        
        var directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0)
        };
        
        foreach (var dir in directions)
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
    
    private AStarNode GetLowestFCost(List<AStarNode> nodes)
    {
        var lowest = nodes[0];
        
        for (int i = 1; i < nodes.Count; i++)
        {
            if (nodes[i].FCost < lowest.FCost)
            {
                lowest = nodes[i];
            }
        }
        
        return lowest;
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
        if (_pathCache.Count >= MAX_CACHE_SIZE)
        {
            _pathCache.Clear();
        }
        
        if (!_pathCache.ContainsKey(start))
        {
            _pathCache[start] = new Dictionary<Vector2Int, List<Vector2Int>>();
        }
        
        _pathCache[start][end] = new List<Vector2Int>(path);
    }
    
    public void InvalidateCache()
    {
        _pathCache.Clear();
        Debug.Log("[PathfindingSystem] Path cache invalidated");
    }
    
    private class AStarNode
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
    }
}