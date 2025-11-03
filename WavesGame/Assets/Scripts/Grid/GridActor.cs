using System;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Grid
{
    public class GridActor : MonoBehaviour
    {
        [SerializeField, ReadOnly] private GridUnit currentUnit;
        [SerializeField] private bool blockGridUnit;

        public virtual void MoveTo(GridUnit unit, Action onFinishMoving, bool animate = false, float time = 0.5f)
        {
            if (currentUnit != null)
            {
                currentUnit.RemoveActor(this);
            }
            unit.AddActor(this);

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

        public bool BlockGridUnit => blockGridUnit;
        public GridUnit GetUnit() => currentUnit;
        public void SetUnit(GridUnit unit) => currentUnit = unit;
    }
}