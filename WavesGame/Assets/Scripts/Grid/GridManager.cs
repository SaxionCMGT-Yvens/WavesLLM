using System;
using System.Collections.Generic;
using UnityEngine;
using UUtils;

namespace Grid
{
    public class GridManager : WeakSingleton<GridManager>
    {
        [SerializeField] private List<GridUnit> gridUnits;
        [SerializeField] private TilemapInfo tilemapInfo;

        [Header("Visuals")] [SerializeField] private List<GridWalkingVisual> visuals;

        private GridUnit[,] _grid;

        protected override void Awake()
        {
            base.Awake();
            AssessUtils.CheckRequirement(ref tilemapInfo, this);
        }

        private void Start()
        {
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            var dimensions = tilemapInfo.GetDimensions();
            var bounds = tilemapInfo.GetTileMapBounds();
            _grid = new GridUnit[dimensions.x, dimensions.y];
            gridUnits.ForEach(unit =>
            {
                var index = GetUnitPosition(unit, dimensions, bounds);
                _grid[index.x, index.y] = unit;
                unit.SetIndex(index);
            });
        }

        public GridUnit GetGridPosition(Transform targetTransform)
        {
            var dimensions = tilemapInfo.GetDimensions();
            var bounds = tilemapInfo.GetTileMapBounds();
            var cellWidth = bounds.size.x / dimensions.x;
            var cellHeight = bounds.size.y / dimensions.y;
            var localOffset = targetTransform.position - bounds.min;
            var gridX = Mathf.FloorToInt(localOffset.x / cellWidth);
            var gridY = Mathf.FloorToInt(localOffset.y / cellHeight);
            CheckGridPosition(new Vector2Int(gridX, gridY), out var gridUnit);
            return gridUnit;
        }

        private static Vector2Int GetUnitPosition(GridUnit unit, Vector2Int dimensions, Bounds tileBounds)
        {
            var cellWidth = tileBounds.size.x / dimensions.x;
            var cellHeight = tileBounds.size.y / dimensions.y;
            var localOffset = unit.transform.position - tileBounds.min;
            var gridX = Mathf.FloorToInt(localOffset.x / cellWidth);
            var gridY = Mathf.FloorToInt(localOffset.y / cellHeight);
            return new Vector2Int(gridX, gridY);
        }

        public void AddGridUnit(GridUnit unit)
        {
            gridUnits.Add(unit);
        }

        public bool CheckGridPosition(Vector2Int position, out GridUnit unit)
        {
            var checkValidPosition = GetValidGridPosition(position, out var validPosition);
            unit = _grid[validPosition.x, validPosition.y];
            return checkValidPosition;
        }

        private bool GetValidGridPosition(Vector2Int position, out Vector2Int validPosition)
        {
            validPosition = position;
            if (CheckPosition(position)) return true;
            validPosition.x = position.x < 0 ? 0 :
                position.x > _grid.GetLength(0) ? _grid.GetLength(0) - 1 : position.x;
            validPosition.y = position.y < 0 ? 0 :
                position.y > _grid.GetLength(0) ? _grid.GetLength(0) - 1 : position.y;
            return false;
        }

        private bool CheckPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x <= _grid.GetLength(0) && position.y >= 0 &&
                   position.y <= _grid.GetLength(1);
        }

        public List<GridUnit> GetGridUnitsInRadiusManhattan(Vector2Int position, int radius)
        {
            DebugUtils.DebugLogMsg("Start Grid Manhattan Area.", DebugUtils.DebugType.Temporary);
            var inRadius = new List<GridUnit>();
            GetValidGridPosition(position, out var validPosition);
            var startUnit = _grid[validPosition.x, validPosition.y];
            
            // ReSharper disable once UseObjectOrCollectionInitializer
            var toVisit = new List<Tuple<GridUnit, int>>();
            var visited = new HashSet<GridUnit>();
            toVisit.Add(new Tuple<GridUnit, int>(startUnit, radius));
            DebugUtils.DebugLogMsg($"Start from first node {startUnit} [{visited.Count}].", DebugUtils.DebugType.Temporary);
            while (toVisit.Count > 0)
            {
                var unitTuple = toVisit[0];
                
                // Stop searching if the unit has been visited already
                var gridUnit = unitTuple.Item1;
                // ReSharper disable once CanSimplifySetAddingWithSingleCall
                if (visited.Contains(gridUnit))
                {
                    toVisit.RemoveAt(0);    
                    continue;
                }
                visited.Add(gridUnit);
                DebugUtils.DebugLogMsg($"Check node - {gridUnit} {gridUnit.Index()} [{visited.Count}].", DebugUtils.DebugType.Temporary);
                
                var index = gridUnit.Index();
                var currentRadius = unitTuple.Item2;
                toVisit.RemoveAt(0);

                // Stop searching once the current radius ends
                if (currentRadius < 0) continue;

                var firstUnit = gridUnit == startUnit;
                
                // Skip this tuple if it is blocked and it is not the current/initial unit
                if (!firstUnit && gridUnit.Type() == GridUnitType.Blocked) continue;
                inRadius.Add(gridUnit);    
                
                DebugUtils.DebugLogMsg($"Visiting next nodes from {gridUnit} {gridUnit.Index()} [{visited.Count}].", DebugUtils.DebugType.Temporary);
                var newRadius = currentRadius - 1;
                VisitNextNodeAt(new Vector2Int(index.x, index.y + 1), newRadius);
                VisitNextNodeAt(new Vector2Int(index.x, index.y - 1), newRadius);
                VisitNextNodeAt(new Vector2Int(index.x + 1, index.y), newRadius);
                VisitNextNodeAt(new Vector2Int(index.x - 1, index.y), newRadius);
            }
            
            return inRadius;

            void VisitNextNodeAt(Vector2Int index, int currentRadius)
            {
                if (CheckPosition(index)) { toVisit.Add(new Tuple<GridUnit, int>(_grid[index.x, index.y], currentRadius)); }
            }
        }
        
        public List<GridUnit> GetGridUnitsInRadius(Vector2Int position, int radius)
        {
            var inRadius = new List<GridUnit>();
            GetValidGridPosition(position, out var validPosition);
            var worldPosition = _grid[validPosition.x, validPosition.y].transform.position;
            DebugUtils.DebugArea(worldPosition, radius);
            DebugUtils.DebugCircle(worldPosition, Color.white, radius);
            for (var i = validPosition.x - radius; i <= validPosition.x + radius; i++)
            {
                for (var j = validPosition.y - radius; j <= validPosition.y + radius; j++)
                {
                    if (!CheckPosition(new Vector2Int(i, j)))
                        continue;

                    var dx = i - validPosition.x;
                    var dy = j - validPosition.y;
                    var distance = Mathf.Sqrt(dx * dx + dy * dy);
                    if (distance <= radius)
                    {
                        inRadius.Add(_grid[i, j]);
                    }
                }
            }

            return inRadius;
        }

        public List<GridUnit> GetManhattanPathFromTo(Vector2Int from, Vector2Int to, bool checkBlocked = false)
        {
            return new List<GridUnit>();
        }

        public Sprite GetSpriteForType(GridUnitType type)
        {
            return type switch
            {
                GridUnitType.Moveable => visuals.Find(unit => unit.type == GridUnitType.Moveable).sprite,
                GridUnitType.Blocked => visuals.Find(unit => unit.type == GridUnitType.Blocked).sprite,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}