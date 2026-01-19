/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 *
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Actors.Cannon;
using Unity.VisualScripting;
using UnityEngine;
using UUtils;

namespace Grid
{
    internal class AStarNode
    {
        public Vector2Int Position { get; }
        public int Cost { get; set; }
        private int Heuristic { get; }
        public int TotalCost => Cost + Heuristic;
    
        public AStarNode(Vector2Int position, int cost, int heuristic)
        {
            Position = position;
            Cost = cost;
            Heuristic = heuristic;
        }
    }

    /// <summary>
    /// The Grid Manager is a weak singleton that controls the current grid.
    /// It keeps track of all grid units (tiles) and the 2D matrix of grid units.
    /// </summary>
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
                var index = GetUnitPosition(unit, bounds);
                _grid[index.x, index.y] = unit;
                unit.SetIndex(index);
            });
            return;

            Vector2Int GetUnitPosition(GridUnit unit, Bounds tileBounds)
            {
                var cellWidth = tileBounds.size.x / dimensions.x;
                var cellHeight = tileBounds.size.y / dimensions.y;
                var localOffset = unit.transform.position - tileBounds.min;
                var gridX = Mathf.FloorToInt(localOffset.x / cellWidth);
                var gridY = Mathf.FloorToInt(localOffset.y / cellHeight);
                return new Vector2Int(gridX, gridY);
            }
        }

        /// <summary>
        /// Returns the grid unit for the position encoded in the targetTransform.
        /// </summary>
        /// <param name="targetTransform">Transform with the position to be used.</param>
        /// <returns>Grid Unit that more closely matches the position.</returns>
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

        public void AddGridUnit(GridUnit unit)
        {
            gridUnits.Add(unit);
        }

        /// <summary>
        /// Checks whether the given position is within a valid grid range.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="unit">Valid GridUnit for the position checked.</param>
        /// <returns>True if the position is valid for the grid range.</returns>
        public bool CheckGridPosition(Vector2Int position, out GridUnit unit)
        {
            var checkValidPosition = GetValidGridPosition(position, out var validPosition);
            unit = _grid[validPosition.x, validPosition.y];
            return checkValidPosition;
        }

        /// <summary>
        /// Checks if a given position is valid within the grid range.
        /// If the position is not valid, then the closest valid position is used and output as the validPosition.
        /// If the position is valid, the validPosition will be the same as the position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="validPosition"></param>
        /// <returns>True if the position is valid.</returns>
        private bool GetValidGridPosition(Vector2Int position, out Vector2Int validPosition)
        {
            validPosition = position;
            if (CheckPosition(position)) return true;
            validPosition.x = position.x < 0 ? 0 :
                position.x >= _grid.GetLength(0) ? _grid.GetLength(0) - 1 : position.x;
            validPosition.y = position.y < 0 ? 0 :
                position.y >= _grid.GetLength(0) ? _grid.GetLength(0) - 1 : position.y;
            return false;
        }

        /// <summary>
        /// Checks if a position is within the grid range.
        /// </summary>
        /// <param name="position"></param>
        /// <returns>True if the position is within the grid range.</returns>
        private bool CheckPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < _grid.GetLength(0) && position.y >= 0 &&
                   position.y < _grid.GetLength(1);
        }

        public List<GridUnit> GetAttackableUnitsInRadiusManhattan(Vector2Int position, CannonSo cannonSo, int radius)
        {
            var attackableHash = new HashSet<GridUnit>();
            var walkableUnits = GetGridUnitsInRadiusManhattan(position, radius, true);
            attackableHash.AddRange(walkableUnits);
            foreach (var positions in walkableUnits.Select(unit => GetGridUnitsForMoveType(cannonSo.targetAreaType, unit.Index(), cannonSo.area,
                         cannonSo.deadZone)))
            {
                attackableHash.AddRange(positions);
            }
            return attackableHash.ToList();
        }

        public bool CanAttackInRadiusManhattan(Vector2Int selfPosition, Vector2Int position, CannonSo cannonSo)
        {
            var attackable =
                GetGridUnitsForMoveType(cannonSo.targetAreaType, selfPosition, cannonSo.area, cannonSo.deadZone);
            var canAttack = attackable.Find(unit => unit.Index().Equals(position));
            return canAttack != null;
        }
        
        /// <summary>
        /// Returns a list of GridUnit there are within a given radius using manhattan distance.
        /// Does not include GridUnits that are unreachable and blocked reachable units.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        /// <returns>List of valid GridUnits reachable and unblocked in the given radius.</returns>
        public List<GridUnit> GetGridUnitsInRadiusManhattan(Vector2Int position, int radius, bool ignoreBlocked = false)
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
                DebugUtils.DebugLogMsg($"Check node - {gridUnit} [{visited.Count}].",
                    DebugUtils.DebugType.Verbose);

                var index = gridUnit.Index();
                var currentRadius = unitTuple.Item2;
                toVisit.RemoveAt(0);

                // Stop searching once the current radius ends
                if (currentRadius < 0) continue;

                var firstUnit = gridUnit == startUnit;

                // Either ignore blocked units or skip this tuple if it is blocked and it is not the current/initial unit,
                if (!ignoreBlocked && !firstUnit && gridUnit.Type() == GridUnitType.Blocked) continue;
                inRadius.Add(gridUnit);

                DebugUtils.DebugLogMsg($"Visiting next nodes from {gridUnit} [{visited.Count}].",
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
        
        public List<GridUnit> GetManhattanPathFromToAStar(Vector2Int from, Vector2Int to, int maxSteps, bool checkBlocked = false)
        {
            var openSet = new List<AStarNode>();
            var closedSet = new HashSet<Vector2Int>();
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            
            var startNode = new AStarNode(from, 0, ManhattanDistance(from, to));
            openSet.Add(startNode);
            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(node => node.TotalCost).First();
                if (current.Position == to)
                {
                    return ReconstructPath(cameFrom, current.Position, from);
                }
                
                openSet.Remove(current);
                closedSet.Add(current.Position);
                if (current.Cost >= maxSteps)
                {
                    continue;
                }
                Vector2Int[] moves = { new(-1, 0), new(1, 0), new(0, -1), new(0, 1) };
                
                foreach (var move in moves)
                {
                    var neighborPos = current.Position + move;
                    if (!GetValidGridPosition(neighborPos, out var validPos) || closedSet.Contains(validPos))
                    {
                        continue;
                    }
                    if (checkBlocked)
                    {
                        var neighborUnit = _grid[validPos.x, validPos.y];
                        if (neighborUnit.Type() == GridUnitType.Blocked)
                        {
                            continue;
                        }
                    }
                    var tentativeG = current.Cost + 1;
                    var existingNode = openSet.FirstOrDefault(n => n.Position == validPos);
                    if (existingNode != null && tentativeG >= existingNode.Cost) continue;
                    if (existingNode == null)
                    {
                        var hCost = ManhattanDistance(validPos, to);
                        var newNode = new AStarNode(validPos, tentativeG, hCost);
                        openSet.Add(newNode);
                    }
                    else
                    {
                        existingNode.Cost = tentativeG;
                    }
                    cameFrom[validPos] = current.Position;
                }
            }
            
            // No path found
            DebugUtils.DebugLogMsg($"Could not find path from {from} to {to}.", DebugUtils.DebugType.Error);
            return new List<GridUnit>();
            
            List<GridUnit> ReconstructPath(Dictionary<Vector2Int, Vector2Int> unitCameFrom, Vector2Int current, Vector2Int start)
            {
                var path = new List<GridUnit>();
                while (current != start)
                {
                    path.Add(_grid[current.x, current.y]);
                    current = unitCameFrom[current];
                }
                path.Add(_grid[start.x, start.y]);
                path.Reverse();
                return path;
            }
        }
        
        private static int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
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
        
        public List<GridUnit> Grid() => gridUnits;
    }
}