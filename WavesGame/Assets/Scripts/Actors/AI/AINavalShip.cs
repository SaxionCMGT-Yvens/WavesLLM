/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 *
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System.Collections;
using Core;
using UnityEngine;
using UUtils;

namespace Actors.AI
{
    public class AINavalShip : NavalShip
    {
        [SerializeField] private AIGenesSO genesData;
        [SerializeField] private AIFaction faction;

        private AIBrain _brain;
        private bool _calculatingAction;

        protected override void Awake()
        {
            base.Awake();
            _brain = new AIBrain(this, navalCannon.GetCannonSo);
        }

        public override void StartTurn()
        {
            base.StartTurn();
            CursorController.GetSingleton().ToggleActive(false);
            StartCoroutine(TurnAI());
        }

        public override void EndTurn()
        {
            base.EndTurn();
            CursorController.GetSingleton().ToggleActive(true);
        }

        private void FinishAITurn()
        {
            LevelController.GetSingleton().EndTurnForCurrentActor();
            DebugUtils.DebugLogMsg($"{name} has finished its turn.", DebugUtils.DebugType.System);
        }

        private IEnumerator TurnAI()
        {
            yield return new WaitForSeconds(0.05f);

            var canCalculateMove = _brain.CalculateMovement(currentUnit.Index(), stepsAvailable, out var chosenAction);
            
            //TODO change this so we can give it a time after the attack before ending the turn.
            if (canCalculateMove)
            {
                _calculatingAction = true;
                var attacked = false;
                DebugUtils.DebugLogMsg($"{name} has selected action {chosenAction}.", DebugUtils.DebugType.System);
                MoveTo(chosenAction.GetUnit(), unit =>
                {
                    //Use all actions
                    while (TryToAct())
                    {
                        var canCalculateAction = _brain.CalculateAction(unit.Index(), out chosenAction);
                        if (!canCalculateAction) continue;
                        var targetUnit = chosenAction.GetUnit();
                        if(targetUnit.ActorsCount() <= 0) continue;
                        DebugUtils.DebugLogMsg($"{name} attacks {chosenAction}!", DebugUtils.DebugType.System);
                        var damage = CalculateDamage();
                        targetUnit.DamageActors(damage);
                        attacked = true;
                    }

                    _calculatingAction = false;
                }, true);
                
                yield return new WaitUntil(() => !_calculatingAction);
                if (attacked)
                {
                    yield return new WaitForSeconds(1.25f);
                }
                FinishAITurn();
            }
            else
            {
                var moveTo = AIBrain.GenerateRandomMovement(currentUnit.Index(), stepsAvailable);
                MoveTo(moveTo, unit => { FinishAITurn(); }, true);
            }
        }

        public AIFaction GetFaction() => faction;
        public AIGenesSO GetGenesData() => genesData;
    }
}