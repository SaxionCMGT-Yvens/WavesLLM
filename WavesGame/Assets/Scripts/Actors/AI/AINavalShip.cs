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
    public class AINavalShip : AIBaseShip
    {
        [SerializeField] private AIGenesSO genesData;
        [SerializeField] private int overrideInitiative;

        private AIBrain _brain;
        private bool _calculatingAction;

        protected override void Awake()
        {
            base.Awake();
            _brain = new AIBrain(this, navalCannon.GetCannonSo);
        }

        protected override void Start()
        {
            base.Start();
            UpdateName();
        }

        protected override IEnumerator TurnAI()
        {
            yield return new WaitForSeconds(0.05f);

            LevelController.GetSingleton().AddInfoLog($"Start turn", name);
            var canCalculateMove = _brain.CalculateMovement(currentUnit.Index(), stepsAvailable, out var chosenAction);
            LevelController.GetSingleton().AddInfoLog($"Can calculate move: {canCalculateMove}", name);
            
            yield return new WaitForSeconds(0.25f);
            if (canCalculateMove)
            {
                _calculatingAction = true;
                var attacked = false;
                DebugUtils.DebugLogMsg($"{name} has selected action {chosenAction}.", DebugUtils.DebugType.System);
                LevelController.GetSingleton().AddInfoLog($"Chosen action: {chosenAction}", name);
                MoveTo(chosenAction.GetUnit(), unit =>
                {
                    while (TryToAct())
                    {
                        var canCalculateAction = _brain.CalculateAction(unit.Index(), out chosenAction);
                        LevelController.GetSingleton().AddInfoLog($"Can calculate action: {canCalculateAction}", name);
                        if (!canCalculateAction) continue;
                        var targetUnit = chosenAction.GetUnit();
                        if(targetUnit.ActorsCount() <= 0) continue;
                        DebugUtils.DebugLogMsg($"{name} attacks {chosenAction}!", DebugUtils.DebugType.System);
                        var damage = CalculateDamage();
                        kills = targetUnit.DamageActors(damage);
                        LevelController.GetSingleton().AddAttackLog(chosenAction.GetUnit().Index(), this, name);
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
                LevelController.GetSingleton().AddInfoLog($"Cannot calculate - random movement!", name);
                var moveTo = AIBrain.GenerateRandomMovement(currentUnit.Index(), stepsAvailable);
                MoveTo(moveTo, _ => { FinishAITurn(); }, true);
            }
        }

        private void UpdateName()
        {
            var internalIDStr = internalID.ToString();
            var factionName = GetFaction().name;
            name = $"LLMAgent|Utility|{factionName}|{internalIDStr}";
        }

        public override string ToString()
        {
            return $"Utility-{genesData.name}";
        }

        public AIGenesSO GetGenesData() => genesData;
    }
}