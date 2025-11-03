using Grid;
using UnityEngine;

namespace Actors
{
    public class NavalShip : NavalActor
    {
        [SerializeField] private NavalShipSo shipData;

        private void Start()
        {
            SetUnit(GridManager.GetSingleton().GetGridPosition(transform));
            //Adjust position to match the grid precisely
            var gridUnit = GetUnit();
            transform.position = gridUnit.transform.position;
            gridUnit.AddActor(this);
        }

        public NavalShipSo ShipData => shipData;
    }
}