using System;
using System.Collections;
using FALLA;
using FALLA.Helper;
using Grid;
using Newtonsoft.Json;
using UnityEngine;
using UUtils;

namespace Actors.AI.LlmAI
{
    [Serializable]
    internal class LlmAction
    {
        public string reasoning;
        public int[] movement;
        public int[] attack;
        public int[] move_after_attack;

        public static Vector2Int GetAsVector2Int(int[] pair)
        {
            return new Vector2Int(pair[0], pair[1]);
        } 
    }

    public class LlmAINavalShip : AIBaseShip
    {
        [Header("LLM")] [SerializeField] private LlmCallerObject llmCaller;
        [SerializeField] private LlmPromptSo basePrompt;

        protected override void Awake()
        {
            base.Awake();
            AssessUtils.CheckRequirement(ref llmCaller, this);
        }

        private static bool IsValidLlmAction(Vector2Int action)
        {
            return action.x != -1 && action.y != -1;
        }

        protected override IEnumerator TurnAI()
        {
            yield return new WaitForSeconds(0.05f);

            var prompt = LlmAiPromptGenerator.GeneratePrompt(this, basePrompt);
            DebugUtils.DebugLogMsg(prompt, DebugUtils.DebugType.Temporary);
            llmCaller.CallLlm(prompt);
            yield return new WaitUntil(() => llmCaller.IsReady());
            var result = llmCaller.GetResponse();
            DebugUtils.DebugLogMsg(result, DebugUtils.DebugType.Temporary);

            var jsonResult = Sanitizer.ExtractJson(result);
            DebugUtils.DebugLogMsg(jsonResult, DebugUtils.DebugType.System);
            
            var actions = JsonConvert.DeserializeObject<LlmAction>(jsonResult);

            DebugUtils.DebugLogMsg(actions.reasoning, DebugUtils.DebugType.System);
            
            var movement = LlmAction.GetAsVector2Int(actions.movement);
            var attack = LlmAction.GetAsVector2Int(actions.attack);
            var moveAfterAttack = LlmAction.GetAsVector2Int(actions.move_after_attack);
            var shouldMove = IsValidLlmAction(movement);
            var shouldAttack = IsValidLlmAction(attack);
            var shouldMoveAfterAttack = IsValidLlmAction(moveAfterAttack);

            if (shouldMove)
            {
                yield return StartCoroutine(LlmMoveCoroutine(movement));
            }

            if (shouldAttack)
            {
                yield return StartCoroutine(LlmAttackCoroutine(attack));
            }

            if (shouldMoveAfterAttack)
            {
                yield return StartCoroutine(LlmMoveCoroutine(moveAfterAttack));
            }

            FinishAITurn();
        }

        private IEnumerator LlmMoveCoroutine(Vector2Int moveToPosition)
        {
            var canMove = GridManager.GetSingleton().CheckGridPosition(moveToPosition, out var moveGridUnit);
            var finishedMoving = false;
            if (canMove)
            {
                MoveTo(moveGridUnit, unit => { finishedMoving = true; }, true);
            }
            else
            {
                DebugUtils.DebugLogErrorMsg($"Could not move to {moveToPosition}.");
                finishedMoving = true;
            }

            yield return new WaitUntil(() => finishedMoving);
        }

        private IEnumerator LlmAttackCoroutine(Vector2Int attackPosition)
        {
            while (TryToAct())
            {
                var hasValidTarget = GridManager.GetSingleton().CheckGridPosition(attackPosition, out var targetUnit);
                if (!hasValidTarget && targetUnit.ActorsCount() <= 0) continue;

                var canAttack = GridManager.GetSingleton()
                    .CanAttackInRadiusManhattan(currentUnit.Index(), attackPosition, navalCannon.GetCannonSo);
                if (canAttack)
                {   
                    DebugUtils.DebugLogMsg($"{name} attacks {targetUnit}!", DebugUtils.DebugType.System);
                    var damage = CalculateDamage();
                    kills = targetUnit.DamageActors(damage);
                    yield return new WaitForSeconds(1.25f);
                }
                else
                {
                    DebugUtils.DebugLogErrorMsg($"Cannot reach target at {targetUnit}.");
                }
            }

            yield return null;
        }
    }
}