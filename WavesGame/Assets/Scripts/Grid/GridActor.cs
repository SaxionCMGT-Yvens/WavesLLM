using System;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Grid
{
    public class GridActor : MonoBehaviour
    {
        [Header("Data")] [SerializeField] private int maxHealth;

        [Header("References")] [SerializeField, ReadOnly]
        private GridUnit currentUnit;

        [SerializeField] private bool blockGridUnit;

        protected virtual void Start()
        {
            SetUnit(GridManager.GetSingleton().GetGridPosition(transform));
            //Adjust position to match the grid precisely
            var gridUnit = GetUnit();
            transform.position = gridUnit.transform.position;
            gridUnit.AddActor(this);
        }

        public virtual void MoveTo(GridUnit unit, Action onFinishMoving, bool animate = false, float time = 0.5f)
        {
            UpdateUnitOnMovement(unit);

            if (animate)
            {
                transform.DOMove(unit.transform.position, time).OnComplete(() => { onFinishMoving?.Invoke(); });
            }
            else
            {
                transform.position = unit.transform.position;
                onFinishMoving?.Invoke();
            }
        }

        protected void UpdateUnitOnMovement(GridUnit unit)
        {
            if (currentUnit != null)
            {
                currentUnit.RemoveActor(this);
            }

            //Adding the actor to the unit also updates the actor's current unit
            unit.AddActor(this);
        }

        public bool BlockGridUnit => blockGridUnit;
        public GridUnit GetUnit() => currentUnit;
        public void SetUnit(GridUnit unit) => currentUnit = unit;
    }
}