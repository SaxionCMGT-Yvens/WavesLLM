using System.Collections.Generic;
using UnityEngine;
using UUtils;

namespace Grid
{
    public class GridManager : WeakSingleton<GridManager>
    {
        [SerializeField] private List<GridUnit> gridUnits;
        [SerializeField] private TilemapInfo tilemapInfo;
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
    }
}