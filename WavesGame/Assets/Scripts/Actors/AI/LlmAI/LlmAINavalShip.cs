using System;
using System.Collections;
using FALLA;
using Grid;
using Newtonsoft.Json;
using UnityEngine;
using UUtils;

namespace Actors.AI.LlmAI
{
    [Serializable]
    internal class LlmAction
    {
        public Vector2Int movement;
        public Vector2Int attack;
        public Vector2Int move_after_attack;
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
            DebugUtils.DebugLogMsg(prompt, DebugUtils.DebugType.Verbose);
            llmCaller.CallLlm(prompt);
            yield return new WaitUntil(() => llmCaller.IsReady());
            var result = llmCaller.GetResponse();
            DebugUtils.DebugLogMsg(result, DebugUtils.DebugType.Verbose);
            var actions = JsonConvert.DeserializeObject<LlmAction>(result);

            var shouldMove = IsValidLlmAction(actions.movement);
            var shouldAttack = IsValidLlmAction(actions.attack);
            var shouldMoveAfterAttack = IsValidLlmAction(actions.move_after_attack);

            if (shouldMove)
            {
                yield return StartCoroutine(LlmMoveCoroutine(actions.movement));
            }

            if (shouldAttack)
            {
                yield return StartCoroutine(LlmAttackCoroutine(actions.attack));
            }

            if (shouldMoveAfterAttack)
            {
                yield return StartCoroutine(LlmMoveCoroutine(actions.move_after_attack));
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
                DebugUtils.DebugLogMsg($"{name} attacks {targetUnit}!", DebugUtils.DebugType.System);
                var damage = CalculateDamage();
                kills = targetUnit.DamageActors(damage);
                yield return new WaitForSeconds(1.25f);
            }

            yield return null;
        }
    }
}