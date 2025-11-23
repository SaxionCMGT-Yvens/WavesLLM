using System;
using System.Collections.Generic;
using UnityEngine;
using UUtils;

namespace Grid
{
    internal class GridUnitVisitRegistry
    {
        private readonly Vector2Int _gridPos;
        public bool visiting = false;
        public bool fullyVisited = false;
        private readonly HashSet<Vector2Int> _visited;

        public GridUnitVisitRegistry(Vector2Int gridPos)
        {
            _gridPos = gridPos;
            _visited = new HashSet<Vector2Int>();
        }

        public void VisitLocation(Vector2Int location) => _visited.Add(location);
        public bool CheckVisitedLocation(Vector2Int location) => _visited.Contains(location);

        public override string ToString()
        {
            return $"<{_gridPos}: V:{visiting} | FV:{fullyVisited} | H:{_visited.Count}>";
        }
    }

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
            DebugUtils.DebugLogMsg("Start Grid Manhattan Area.", DebugUtils.DebugType.Verbose);
            var inRadius = new List<GridUnit>();
            GetValidGridPosition(position, out var validPosition);
            var startUnit = _grid[validPosition.x, validPosition.y];

            // ReSharper disable once UseObjectOrCollectionInitializer
            var toVisit = new List<Tuple<GridUnit, int>>();
            var visited = new HashSet<GridUnit>();
            toVisit.Add(new Tuple<GridUnit, int>(startUnit, radius));
            DebugUtils.DebugLogMsg($"Start from first node {startUnit.Index()} [{visited.Count}].",
                DebugUtils.DebugType.Verbose);
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
                    DebugUtils.DebugType.Verbose);

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
                    DebugUtils.DebugType.Verbose);
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

        public List<GridUnit> GetGridUnitsForMoveType(GridMoveType moveType, Vector2Int position, int distance,
            int deadZone = 0)
        {
            var units = new List<GridUnit>();
            switch (moveType)
            {
                case GridMoveType.Cross:
                    GridMoveUp();
                    GridMoveDown();
                    GridMoveLeft();
                    GridMoveRight();
                    break;
                case GridMoveType.Area:
                    units.AddRange(GetGridUnitsInRadius(position, distance, deadZone));
                    break;
                case GridMoveType.Diagonal:
                    GridUnitsInDiagonal();
                    break;
                case GridMoveType.Up:
                    GridMoveUp();
                    break;
                case GridMoveType.Down:
                    GridMoveDown();
                    break;
                case GridMoveType.Left:
                    GridMoveLeft();
                    break;
                case GridMoveType.Right:
                    GridMoveRight();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(moveType), moveType, null);
            }

            return units;

            void GridUnitsInDiagonal()
            {
                DiagonalMove(1, 1);
                DiagonalMove(-1, 1);
                DiagonalMove(-1, -1);
                DiagonalMove(1, -1);
                return;

                void DiagonalMove(int signX, int signY)
                {
                    var to = new Vector2Int(position.x + signX * distance, position.y + signY * distance);
                    GetValidGridPosition(to, out var validTo);
                    var steps = Mathf.Abs(validTo.x - position.x);
                    steps -= deadZone;
                    while (steps > 0)
                    {
                        units.Add(_grid[validTo.x, validTo.y]);
                        --steps;
                        validTo.x -= signX;
                        validTo.y -= signY;
                    }
                }
            }

            List<GridUnit> GridUnitsInLine(Vector2Int from, Vector2Int to, int validDistance)
            {
                var result = new List<GridUnit>();
                if (validDistance <= 0) return result;
                var numberOfUnits = validDistance - deadZone;
                if (numberOfUnits <= 0) return result;

                //Start from the back to skip the deadZone
                var current = to;
                while (numberOfUnits > 0)
                {
                    result.Add(_grid[current.x, current.y]);
                    --numberOfUnits;
                    if (current.x != from.x)
                    {
                        current.x += (from.x < current.x) ? -1 : 1;
                    }
                    else
                    {
                        current.y += (from.y < current.y) ? -1 : 1;
                    }
                }

                return result;
            }

            void GridMoveUp()
            {
                var upPosition = new Vector2Int(position.x, position.y + distance);
                GetValidGridPosition(upPosition, out var validUpPosition);
                var validDistance = Mathf.Abs(validUpPosition.y - position.y);
                units.AddRange(GridUnitsInLine(position, validUpPosition, validDistance));
            }

            void GridMoveDown()
            {
                var downPosition = new Vector2Int(position.x, position.y - distance);
                GetValidGridPosition(downPosition, out var validDownPosition);
                var validDistance = Mathf.Abs(position.y - validDownPosition.y);
                units.AddRange(GridUnitsInLine(position, validDownPosition, validDistance));
            }

            void GridMoveLeft()
            {
                var leftPosition = new Vector2Int(position.x - distance, position.y);
                GetValidGridPosition(leftPosition, out var validLeftPosition);
                var validDistance = Mathf.Abs(position.x - validLeftPosition.x);
                units.AddRange(GridUnitsInLine(position, validLeftPosition, validDistance));
            }

            void GridMoveRight()
            {
                var rightPosition = new Vector2Int(position.x + distance, position.y);
                GetValidGridPosition(rightPosition, out var validRightPosition);
                var validDistance = Mathf.Abs(validRightPosition.x - position.x);
                units.AddRange(GridUnitsInLine(position, validRightPosition, validDistance));
            }
        }

        public List<GridUnit> GetBresenhamLine(Vector2Int from, Vector2Int to)
        {
            var line = new List<GridUnit>();

            var x0 = from.x;
            var y0 = from.y;
            var x1 = to.x;
            var y1 = to.y;

            var steep = Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0);
            if (steep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }

            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            var dx = x1 - x0;
            var dy = Mathf.Abs(y1 - y0);
            var error = dx / 2;
            var yStep = (y0 < y1) ? 1 : -1;
            var y = y0;

            for (var x = x0; x <= x1; x++)
            {
                var point = steep ? new Vector2Int(y, x) : new Vector2Int(x, y);
                if (GetValidGridPosition(point, out var validPosition))
                {
                    line.Add(_grid[validPosition.x, validPosition.y]);
                }

                error -= dy;
                if (error >= 0) continue;
                y += yStep;
                error += dx;
            }

            return line;

            void Swap(ref int a, ref int b)
            {
                (a, b) = (b, a);
            }
        }

        private List<GridUnit> GetGridUnitsInRadius(Vector2Int position, int radius, int deadZone = 0)
        {
            var inRadius = new List<GridUnit>();
            GetValidGridPosition(position, out var validPosition);
            var worldPosition = _grid[validPosition.x, validPosition.y].transform.position;
            DebugUtils.DebugArea(worldPosition, radius);
            DebugUtils.DebugCircle(worldPosition, Color.white, radius);
            DebugUtils.DebugCircle(worldPosition, Color.red, deadZone);

            for (var i = validPosition.x - radius; i <= validPosition.x + radius; i++)
            {
                for (var j = validPosition.y - radius; j <= validPosition.y + radius; j++)
                {
                    if (!CheckPosition(new Vector2Int(i, j)))
                        continue;

                    var dx = i - validPosition.x;
                    var dy = j - validPosition.y;
                    var distance = Mathf.Sqrt(dx * dx + dy * dy);
                    if (distance <= radius && distance > deadZone)
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
            DebugUtils.DebugLogMsg($"Start Path from [{from}] to [{to}] Manhattan.", DebugUtils.DebugType.Verbose);
            GetValidGridPosition(from, out var validPosition);

            var steps = maxSteps;
            var current = validPosition;
            while (steps >= 0)
            {
                DebugUtils.DebugLogMsg($"Current step [{current}] [Step: {steps}].", DebugUtils.DebugType.Verbose);
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
            var visited = new List<GridUnitVisitRegistry>();
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
                var visitRegistry = new GridUnitVisitRegistry(currentUnit.Index())
                {
                    visiting = true
                };
                visited.Add(visitRegistry);

                DebugUtils.DebugLogMsg(
                    $"FPR Current step [{currentUnit.Index()}] [Path Size: {pathFromTo.Count}, Hash Size: {visited.Count}] [Steps: {remainingSteps}].",
                    DebugUtils.DebugType.Verbose);

                // Check if we reached target
                if (current == to)
                {
                    DebugUtils.DebugLogMsg(
                        $"FPR Current {current} equal To {to}. Target reached!",
                        DebugUtils.DebugType.Verbose);
                    return true;
                }

                // Check if we have steps remaining
                if (remainingSteps < 0)
                {
                    pathFromTo.RemoveAt(pathFromTo.Count - 1); // Backtrack
                    DebugUtils.DebugLogMsg(
                        $"FPR Early Backtracking! No more valid steps!",
                        DebugUtils.DebugType.Verbose);
                    return false;
                }

                // Try both X and Y directions
                Vector2Int[] moves =
                {
                    new(-1, 0),
                    new(1, 0),
                    new(0, -1),
                    new(0, 1)
                };

                // Try both moves in a smart order (prioritize the direction with larger difference)
                var xFirst = Mathf.Abs(to.x - current.x) > Mathf.Abs(to.y - current.y);
                if (xFirst)
                {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    for (var i = 0; i < moves.Length; i++)
                        if (TryMove(currentUnit, current, moves[i], visitRegistry, remainingSteps))
                        {
                            return true;
                        }
                }
                else
                {
                    for (var i = moves.Length - 1; i > -1; i--)
                        if (TryMove(currentUnit, current, moves[i], visitRegistry, remainingSteps))
                        {
                            return true;
                        }
                }

                visitRegistry.fullyVisited = true;

                // If no moves work, backtrack
                pathFromTo.RemoveAt(pathFromTo.Count - 1);
                DebugUtils.DebugLogMsg(
                    $"FPR Late Backtracking!",
                    DebugUtils.DebugType.Verbose);
                return false;
            }

            bool TryMove(GridUnit currentUnit, Vector2Int current, Vector2Int move, GridUnitVisitRegistry registry,
                int remainingSteps)
            {
                if (move == Vector2Int.zero) return false;
                var next = current + move;
                if (registry.CheckVisitedLocation(move))
                {
                    //Already checked this direction
                    DebugUtils.DebugLogMsg(
                        $"Cannot move to {next} - registry states that this move has already been fully visited.",
                        DebugUtils.DebugType.Verbose);
                    return false;
                }

                registry.VisitLocation(move);
                var isValidPosition = GetValidGridPosition(next, out var validPosition);
                // Check if the move is valid
                if (isValidPosition)
                {
                    var nextUnit = _grid[validPosition.x, validPosition.y];
                    DebugUtils.DebugLine(currentUnit.transform.position, nextUnit.transform.position, Color.black);
                    var unblockedMovement = (!checkBlocked || nextUnit.Type() != GridUnitType.Blocked);
                    if (!registry.fullyVisited && unblockedMovement)
                    {
                        return FindPathRecursive(validPosition, remainingSteps - 1);
                    }
                }

                DebugUtils.DebugLogMsg(
                    $"Cannot move to {next} - is valid position {isValidPosition}? was visited {registry}? ",
                    DebugUtils.DebugType.Verbose);
                return false;
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