/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 * 
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] protected NavalShipSo shipData;
        [SerializeField] protected BaseCannon navalCannon;

        private int _actions;
        protected int stepsAvailable;

        protected override void Awake()
        {
            base.Awake();
            AssessUtils.CheckRequirement(ref spriteRenderer, this);
        }

        public virtual void StartTurn()
        {
            //Reset turn variables
            _actions = shipData.stats.spirit.Two;
            stepsAvailable = shipData.stats.speed.Two;
        }

        public virtual void EndTurn()
        {
            _actions = 0;
        }

        protected void SetInitiative(int initiative)
        {
            Initiative = initiative;
        }
        
        public void RollInitiative()
        {
            Initiative = shipData.RollInitiative();
        }

        public bool TryToAct()
        {
            var canAct = CanAct();
            if (CanAct()) --_actions;
            return canAct;
        }

        public bool CanAct()
        {
            return _actions > 0;
        }

        public int CalculateDamage()
        {
            return ShipData.stats.strength.Two + NavalCannon.CalculateDamage();
        }

        public override bool TakeDamage(int damage)
        {
            var damageTaken = damage - shipData.stats.sturdiness.Two;
            DebugUtils.DebugLogMsg($"{name} attacked with {damage}. Sturdiness is {shipData.stats.sturdiness.Two}. Damage taken was {damageTaken}.", DebugUtils.DebugType.Verbose);
            return base.TakeDamage(damageTaken);
        }
        
        public override bool MoveTo(GridUnit unit, Action<GridUnit> onFinishMoving, bool animate = false, float time = 0.5f)
        {
            if (animate)
            {
                var steps = GridManager.GetSingleton()
                    .GetManhattanPathFromToAStar(GetUnit().Index(), unit.Index(), stepsAvailable,
                        true);

                
                if (steps.Count == 0)
                {
                    DebugUtils.DebugLogMsg("Path not found! Stay in the same position.", DebugUtils.DebugType.Error);
                    // Calls the callback without a valid return position
                    onFinishMoving?.Invoke(null);
                    return false;
                }
                
                var stepsCount = steps.Count - 1; //Removes the initial (current) step from the movement count.
                stepsAvailable = Mathf.Max(stepsAvailable - stepsCount, 0);
                
                if (steps.Count <= 0)
                {
                    onFinishMoving?.Invoke(unit);
                    return false;
                }
                StartCoroutine(MovementStepsCoroutine(steps, onFinishMoving, time));
            }
            else
            {
                transform.position = unit.transform.position;
                UpdateGridUnitOnMovement(unit);
                onFinishMoving?.Invoke(unit);
            }

            return true;
        }
        
        private IEnumerator MovementStepsCoroutine(List<GridUnit> steps, Action<GridUnit> onFinishMoving, float time)
        {
            var stepsEnumerator = steps.GetEnumerator();
            var continueSteps = true;
            var nextStep = false;
            var cumulativeDamage = 0;
            var finalStep = steps[^1];
            while (continueSteps && stepsEnumerator.MoveNext())
            {
                var current = stepsEnumerator.Current;
                if (current == null) continue;
                nextStep = false;
                transform.DOMove(current.transform.position, time).OnComplete(() =>
                {
                    nextStep = true;
                });
                yield return new WaitUntil(() => nextStep);
                UpdateGridUnitOnMovement(current);
                finalStep = current;
                
                if (!current.HasValidActors()) continue;
                var stepEffects = current.GetHasStepEffectActors();
                if (stepEffects.Count <= 0) continue;
                DebugUtils.DebugLogMsg($"{name} has stepped on something!", DebugUtils.DebugType.Verbose);
                foreach (var effect in stepEffects.Select(stepActor => stepActor.StepEffect(this)))
                {
                    if (effect.causeDamage)
                    {
                        cumulativeDamage += effect.damage;
                    }

                    if (effect.canContinueMovement) continue;
                    DebugUtils.DebugLogMsg($"{name} hit by an effect.", DebugUtils.DebugType.Verbose);
                    
                    var resist = shipData.ResistWave();
                    if (resist) continue;
                    var moveToUnit = effect.moveTo;
                    if (moveToUnit == null) continue;
                    nextStep = false;
                    
                    //TODO for now, it does not check for the case of multiple waves moving the ship
                    transform.DOMove(moveToUnit.transform.position, time).OnComplete(() =>
                    {
                        DebugUtils.DebugLogMsg($"{name} being pushed by the waves to {moveToUnit.Index()}!", DebugUtils.DebugType.Verbose);
                        UpdateGridUnitOnMovement(moveToUnit);
                        finalStep = moveToUnit;
                        continueSteps = false;
                        nextStep = true;
                        //Being pushed back, but it seems to keep on marking the wrong piece in the map
                    });
                    yield return new WaitUntil(() => nextStep);
                }
            }

            if (cumulativeDamage > 0)
            {
                TakeDamage(cumulativeDamage);    
            }
            stepsEnumerator.Dispose();
            DebugUtils.DebugLogMsg($"{name} has final step at {finalStep.Index()}!", DebugUtils.DebugType.Verbose);
            onFinishMoving?.Invoke(finalStep);
        }
        
        protected override void NotifyLevelController()
        {
            LevelController.GetSingleton().NotifyDestroyedActor(this);
        }

        public NavalShipSo ShipData => shipData;
        public BaseCannon NavalCannon => navalCannon;
        public int RemainingSteps => stepsAvailable;
        public int Initiative { get; private set; }
        public int ActionsLeft => _actions;
        public SpriteRenderer Renderer() => spriteRenderer;

        public int CompareTo(NavalShip other)
        {
            if (ReferenceEquals(this, other)) return 0;
            // Sort by the largest to the smallest
            return other is null ? 1 : other.Initiative.CompareTo(other.Initiative);
        }
        
        public override string ToString()
        {
            return $"{base.ToString()}; initiative={Initiative}; ShipData=[{shipData}]; Cannon=[{navalCannon}]";
        }
    }
}