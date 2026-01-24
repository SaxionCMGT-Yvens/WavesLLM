using System;
using System.Collections;
using System.Diagnostics;
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
        public string reasoning = "";
        public int[] movement = new []{-1,-1};
        public int[] attack = new []{-1,-1};
        public int[] moveAfterAttack = new []{-1,-1};

        public static Vector2Int GetAsVector2Int(int[] pair)
        {
            return new Vector2Int(pair[0], pair[1]);
        } 
    }

    public class LlmAINavalShip : AIBaseShip
    {
        [Header("LLM")] 
        [SerializeField] private LlmCallerObject llmCaller;
        [SerializeField] private float requestTimeOutTimer = 1.0f;
        [SerializeField] private LlmPromptSo basePrompt;

        protected override void Awake()
        {
            base.Awake();
            AssessUtils.CheckRequirement(ref llmCaller, this);
        }

        protected override void Start()
        {
            base.Start();
            var llmName = llmCaller.GetLlmType().ToString();
            var factionName = GetFaction().name;
            var internalIDStr = _internalID.ToString();
            name = $"LLMAgent - {llmName} - {factionName} - {internalIDStr}";
        }

        private static bool IsValidLlmAction(Vector2Int action)
        {
            return action.x != -1 && action.y != -1;
        }

        protected override IEnumerator TurnAI()
        {
            yield return new WaitForSeconds(0.05f);
            
            DebugUtils.DebugLogMsg($"Request Timer. Wait for {requestTimeOutTimer} seconds.", DebugUtils.DebugType.System);
            yield return new WaitForSeconds(requestTimeOutTimer);
            DebugUtils.DebugLogMsg($"Request Timer Finished.", DebugUtils.DebugType.System);

            var prompt = LlmAiPromptGenerator.GeneratePrompt(this, basePrompt);
            DebugUtils.DebugLogMsg(prompt, DebugUtils.DebugType.Temporary);
            
            var stopwatch = Stopwatch.StartNew();
            llmCaller.CallLlm(prompt);
            yield return new WaitUntil(() => llmCaller.IsReady());
            var result = llmCaller.GetResponse();
            stopwatch.Stop();
            DebugUtils.DebugLogMsg($"Request response in {stopwatch.ElapsedMilliseconds} ms.", DebugUtils.DebugType.System);

            if (string.IsNullOrEmpty(result))
            {
                DebugUtils.DebugLogMsg("No response returned.", DebugUtils.DebugType.Error);
                FinishAITurn();
                yield break;
            }
            
            DebugUtils.DebugLogMsg(result, DebugUtils.DebugType.Temporary);

            var jsonResult = Sanitizer.ExtractJson(result);
            DebugUtils.DebugLogMsg(jsonResult, DebugUtils.DebugType.System);

            var actions = new LlmAction();
            try
            {
                actions = JsonConvert.DeserializeObject<LlmAction>(jsonResult);    
            } catch (Exception e)
            {
                DebugUtils.DebugLogMsg($"Exception {e.Message}.", DebugUtils.DebugType.Error);
                DebugUtils.DebugLogErrorMsg(e.Message);
            }

            DebugUtils.DebugLogMsg(actions.reasoning, DebugUtils.DebugType.System);

            var shouldMove = false;
            var shouldAttack = false;
            var shouldMoveAfterAttack = false;
            var movement = new Vector2Int(-1, -1);
            var attack = new Vector2Int(-1, -1);
            var moveAfterAttack = new Vector2Int(-1, -1);
            
            try
            {
                movement = LlmAction.GetAsVector2Int(actions.movement);
                attack = LlmAction.GetAsVector2Int(actions.attack);
                moveAfterAttack = LlmAction.GetAsVector2Int(actions.moveAfterAttack);
                shouldMove = IsValidLlmAction(movement);
                shouldAttack = IsValidLlmAction(attack);
                shouldMoveAfterAttack = IsValidLlmAction(moveAfterAttack);
            }
            catch (Exception e)
            {
                DebugUtils.DebugLogMsg($"Exception {e.Message}.", DebugUtils.DebugType.Error);
                DebugUtils.DebugLogErrorMsg(e.Message);
            }
            
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
                MoveTo(moveGridUnit, _ => { finishedMoving = true; }, true);
            }
            else
            {
                DebugUtils.DebugLogMsg($"Could not move to {moveToPosition}.", DebugUtils.DebugType.Error);
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
                    .CanAttackFrom(currentUnit.Index(), attackPosition, navalCannon.GetCannonSo);
                if (canAttack)
                {   
                    DebugUtils.DebugLogMsg($"{name} attacks {targetUnit}!", DebugUtils.DebugType.System);
                    var damage = CalculateDamage();
                    kills = targetUnit.DamageActors(damage);
                    yield return new WaitForSeconds(1.25f);
                }
                else
                {
                    DebugUtils.DebugLogMsg($"Cannot reach target at {targetUnit}.", DebugUtils.DebugType.Error);
                }
            }

            yield return null;
        }
    }
}