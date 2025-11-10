using System;
using Actors.Cannon;
using Core;
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

        private int _actions;
        private int _stepsAvailable;
        
        public void StartTurn()
        {
            //Reset turn variables
            _actions = shipData.stats.spirit.Two;
            _stepsAvailable = shipData.stats.speed.Two;
        }

        public void EndTurn()
        {
            _actions = 0;
        }
        
        public void RollInitiative()
        {
            Initiative = shipData.RollInitiative();
        }

        public override void TakeDamage(int damage)
        {
            var damageTaken = damage - shipData.stats.sturdiness.Two;
            DebugUtils.DebugLogMsg($"{name} attacked with {damage}. Sturdiness is {shipData.stats.sturdiness.Two}. Damage taken was {damageTaken}.", DebugUtils.DebugType.Temporary);
            base.TakeDamage(damageTaken);
        }
        
        public override void MoveTo(GridUnit unit, Action onFinishMoving, bool animate = false, float time = 0.5f)
        {
            if (animate)
            {
                var steps = GridManager.GetSingleton()
                    .GetManhattanPathFromToRecursive(GetUnit().Index(), unit.Index(), _stepsAvailable,
                        true);
                var stepsCount = steps.Count - 1; //Removes the initial (current) step from the movement count.
                _stepsAvailable = Mathf.Max(_stepsAvailable - stepsCount, 0);
                
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
        
        protected override void NotifyLevelController()
        {
            LevelController.GetSingleton().NotifyDestroyedActor(this);
        }

        public NavalShipSo ShipData => shipData;
        public BaseCannon NavalCannon => navalCannon;
        public int RemainingSteps => _stepsAvailable;
        public int Initiative { get; private set; }
        public int ActionsLeft => _actions;

        public int CompareTo(NavalShip other)
        {
            if (ReferenceEquals(this, other)) return 0;
            return other is null ? 1 : Initiative.CompareTo(other.Initiative);
        }
    }
}