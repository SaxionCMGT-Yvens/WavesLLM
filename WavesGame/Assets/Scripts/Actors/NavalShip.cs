using Grid;
using NaughtyAttributes;
using UnityEngine;

namespace Actors
{
    public class NavalShip : NavalActor
    {
        [SerializeField, ReadOnly] private GridUnit currentGridUnit;
        [SerializeField] private NavalShipSo shipData;

        private void Start()
        {
            currentGridUnit = GridManager.GetSingleton().GetGridPosition(transform);
            //Adjust position to match the grid precisely
            transform.position = currentGridUnit.transform.position;
            currentGridUnit.AddActor(this);
        }

        public GridUnit CurrentGridUnit => currentGridUnit;
        public NavalShipSo ShipData => shipData;
    }
}