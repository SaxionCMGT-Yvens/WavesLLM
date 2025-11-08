using System;
using Actors.Cannon;
using DG.Tweening;
using Grid;
using UnityEngine;
using UUtils;

namespace Actors
{
    public class NavalShip : NavalActor, IComparable<NavalShip>
    {
        [SerializeField] private NavalShipSo shipData;
        [SerializeField] private BaseCannon navalCannon;

        public void RollInitiative()
        {
            Initiative = shipData.RollInitiative();
        }

        public override void MoveTo(GridUnit unit, Action onFinishMoving, bool animate = false, float time = 0.5f)
        {
            if (animate)
            {
                var steps = GridManager.GetSingleton()
                    .GetManhattanPathFromToRecursive(GetUnit().Index(), unit.Index(), shipData.movementStepsPerTurn,
                        true);

                if (steps.Count <= 0)
                {
                    onFinishMoving?.Invoke();
                    return;
                }

                var movementSequence = DOTween.Sequence();
                steps.ForEach(step =>
                {
                    DebugUtils.DebugLogMsg($"Path [{step.Index()}]", DebugUtils.DebugType.Temporary);
                    movementSequence.Append(transform.DOMove(step.transform.position, time));
                });
                movementSequence.OnComplete(() =>
                {
                    //Moves to the final step in the sequence.
                    UpdateUnitOnMovement(steps[^1]);
                    onFinishMoving?.Invoke();
                });
                movementSequence.Play();
            }
            else
            {
                transform.position = unit.transform.position;
                UpdateUnitOnMovement(unit);
                onFinishMoving?.Invoke();
            }
        }

        public NavalShipSo ShipData => shipData;
        public BaseCannon NavalCannon => navalCannon;
        public int Initiative { get; private set; }

        public int CompareTo(NavalShip other)
        {
            if (ReferenceEquals(this, other)) return 0;
            return other is null ? 1 : Initiative.CompareTo(other.Initiative);
        }
    }
}