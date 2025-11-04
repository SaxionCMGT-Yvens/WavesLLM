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
            DebugUtils.DebugLogMsg($"Start from first node {startUnit.Index()} [{visited.Count}].",
                DebugUtils.DebugType.Temporary);
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
                DebugUtils.DebugLogMsg($"Check node - {gridUnit.Index()} [{visited.Count}].",
                    DebugUtils.DebugType.Temporary);

                var index = gridUnit.Index();
                var currentRadius = unitTuple.Item2;
                toVisit.RemoveAt(0);

                // Stop searching once the current radius ends
                if (currentRadius < 0) continue;

                var firstUnit = gridUnit == startUnit;

                // Skip this tuple if it is blocked and it is not the current/initial unit
                if (!firstUnit && gridUnit.Type() == GridUnitType.Blocked) continue;
                inRadius.Add(gridUnit);

                DebugUtils.DebugLogMsg($"Visiting next nodes from {gridUnit.Index()} [{visited.Count}].",
                    DebugUtils.DebugType.Temporary);
                var newRadius = currentRadius - 1;
                VisitNextNodeAt(new Vector2Int(index.x, index.y + 1), newRadius);
                VisitNextNodeAt(new Vector2Int(index.x, index.y - 1), newRadius);
                VisitNextNodeAt(new Vector2Int(index.x + 1, index.y), newRadius);
                VisitNextNodeAt(new Vector2Int(index.x - 1, index.y), newRadius);
            }

            return inRadius;

            void VisitNextNodeAt(Vector2Int index, int currentRadius)
            {
                if (CheckPosition(index))
                {
                    toVisit.Add(new Tuple<GridUnit, int>(_grid[index.x, index.y], currentRadius));
                }
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

        //NOTE: use it either as a sort of line of sight or as a "dumb" movement algorithm for the AI ships 
        public List<GridUnit> GetManhattanLineOfSightFromTo(Vector2Int from, Vector2Int to, int maxSteps,
            bool checkBlocked = false)
        {
            //TODO consider a recursive function instead
            var pathFromTo = new List<GridUnit>();
            DebugUtils.DebugLogMsg($"Start Path from [{from}] to [{to}] Manhattan.", DebugUtils.DebugType.Temporary);
            GetValidGridPosition(from, out var validPosition);

            var steps = maxSteps;
            var current = validPosition;
            while (steps >= 0)
            {
                DebugUtils.DebugLogMsg($"Current step [{current}] [Step: {steps}].");
                var currentUnit = _grid[current.x, current.y];
                pathFromTo.Add(currentUnit);
                if (currentUnit.Index() == to)
                {
                    break;
                }

                var possibleMoveOnX =
                    (current.x != to.x) ? (to.x > current.x ? current.x + 1 : current.x - 1) : current.x;
                var isValidX = GetValidGridPosition(new Vector2Int(possibleMoveOnX, current.y), out validPosition)
                               && validPosition != currentUnit.Index()
                               && (!checkBlocked || _grid[validPosition.x, validPosition.y].Type() !=
                                   GridUnitType.Blocked);
                if (isValidX)
                {
                    current.x = possibleMoveOnX;
                    --steps;
                    continue;
                }

                var possibleMoveOnY =
                    (current.y != to.y) ? (to.y > current.y ? current.y + 1 : current.y - 1) : current.y;
                var isValidY = GetValidGridPosition(new Vector2Int(current.x, possibleMoveOnY), out validPosition)
                               && validPosition != currentUnit.Index()
                               && (!checkBlocked || _grid[validPosition.x, validPosition.y].Type() !=
                                   GridUnitType.Blocked);
                if (isValidY)
                {
                    current.y = possibleMoveOnY;
                    --steps;
                    continue;
                }

                DebugUtils.DebugLogMsg($"Could not find path from {from} to {to} [currently at {current}].",
                    DebugUtils.DebugType.Error);
                break;
            }

            return pathFromTo;
        }

        public List<GridUnit> GetManhattanPathFromToRecursive(Vector2Int from, Vector2Int to, int maxSteps,
            bool checkBlocked = false)
        {
            var pathFromTo = new List<GridUnit>();
            var visited = new HashSet<Vector2Int>();
            var success = FindPathRecursive(from, maxSteps);

            if (success)
            {
                return pathFromTo;
            }

            DebugUtils.DebugLogMsg($"Could not find path from {from} to {to}.", DebugUtils.DebugType.Error);
            return new List<GridUnit>();

            bool FindPathRecursive(Vector2Int current, int remainingSteps)
            {
                // Add current position to path
                var currentUnit = _grid[current.x, current.y];
                pathFromTo.Add(currentUnit);
                visited.Add(current);
                DebugUtils.DebugLogMsg(
                    $"FPR Current step [{currentUnit.Index()}] [Path Size: {pathFromTo.Count}, Hash Size: {visited.Count}] [Steps: {remainingSteps}].",
                    DebugUtils.DebugType.Temporary);

                // Check if we reached target
                if (current == to)
                {
                    return true;
                }

                // Check if we have steps remaining
                if (remainingSteps < 0)
                {
                    pathFromTo.RemoveAt(pathFromTo.Count - 1); // Backtrack
                    return false;
                }

                // Try both X and Y directions
                Vector2Int[] moves =
                {
                    new(current.x != to.x ? (to.x > current.x ? 1 : -1) : 0, 0), // X move
                    new(0, current.y != to.y ? (to.y > current.y ? 1 : -1) : 0) // Y move
                };

                // Try both moves in a smart order (prioritize the direction with larger difference)
                var xFirst = Mathf.Abs(to.x - current.x) > Mathf.Abs(to.y - current.y);
                if (TryMove(current, xFirst ? moves[0] : moves[1], remainingSteps))
                    return true;
                if (TryMove(current, xFirst ? moves[1] : moves[0], remainingSteps))
                    return true;

                // If no moves work, backtrack
                pathFromTo.RemoveAt(pathFromTo.Count - 1);
                return false;
            }

            bool TryMove(Vector2Int current, Vector2Int move, int remainingSteps)
            {
                if (move == Vector2Int.zero) return false;

                var next = current + move;
                // Check if move is valid
                if (!GetValidGridPosition(next, out var validPosition) ||
                    visited.Contains(validPosition) ||
                    (checkBlocked && _grid[validPosition.x, validPosition.y].Type() == GridUnitType.Blocked))
                {
                    return false;
                }

                // Recursively try this path
                return FindPathRecursive(validPosition, remainingSteps - 1);
            }
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