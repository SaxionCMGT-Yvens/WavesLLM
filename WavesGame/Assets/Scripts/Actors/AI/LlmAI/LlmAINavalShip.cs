using System;
using System.Collections;
using System.Diagnostics;
using Core;
using FALLA;
using FALLA.Exception;
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
        public int[] movement = new[] { -1, -1 };
        public int[] attack = new[] { -1, -1 };
        public int[] moveAfterAttack = new[] { -1, -1 };

        public static Vector2Int GetAsVector2Int(int[] pair)
        {
            return new Vector2Int(pair[0], pair[1]);
        }
    }

    public class LlmAINavalShip : AIBaseShip
    {
        [Header("LLM")] [SerializeField] private LlmCallerObject llmCaller;
        [SerializeField] private float requestTimeOutTimer = 1.0f;
        [SerializeField] private LlmPromptSo basePrompt;

        private int _internalWrongMovementCount;
        private int _internalWrongAttackCount;
        private int _internalTotalRequestCount;
        private int _internalMovementAttemptCount;
        private int _internalAttackAttemptCount;
        private int _internalFaultyMessageCount;

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
            var internalIDStr = internalID.ToString();
            name = $"LLMAgent - {llmName} - {factionName} - {internalIDStr}";
        }

        private static bool IsValidLlmAction(Vector2Int action)
        {
            return action.x != -1 && action.y != -1;
        }

        protected override IEnumerator TurnAI()
        {
            //Wait two frames for the logger to get ready
            yield return null;
            
            AddInfoLog("Start turn");
            yield return new WaitForSeconds(0.05f);

            DebugUtils.DebugLogMsg($"Request Timer. Wait for {requestTimeOutTimer} seconds.",
                DebugUtils.DebugType.System);
            yield return new WaitForSeconds(requestTimeOutTimer);
            DebugUtils.DebugLogMsg($"Request Timer Finished.", DebugUtils.DebugType.System);

            var prompt = LlmAiPromptGenerator.GeneratePrompt(this, basePrompt);
            DebugUtils.DebugLogMsg(prompt, DebugUtils.DebugType.Temporary);

            bool retry;
            var maxAttempts = 5;
            var breakTime = 5.0f;
            var result = "";
            do
            {
                var stopwatch = Stopwatch.StartNew();
                DebugUtils.DebugLogMsg("Prompt sent...", DebugUtils.DebugType.Temporary);
                llmCaller.CallLlm(prompt);
                _internalTotalRequestCount++;
                yield return new WaitUntil(() => llmCaller.IsReady());
                try
                {
                    result = llmCaller.GetResponse();
                }
                catch (NoResponseException noResponseException)
                {
                    DebugUtils.DebugLogMsg($"No response exception: {noResponseException.Message}.", DebugUtils.DebugType.Error);
                    AddInfoLog($"No response exception! {noResponseException.Message}");
                    _internalFaultyMessageCount++;
                    result = "";
                }
                
                DebugUtils.DebugLogMsg($"Result received: [{result}].", DebugUtils.DebugType.Temporary);

                if (string.IsNullOrEmpty(result))
                {
                    DebugUtils.DebugLogMsg("No response returned.", DebugUtils.DebugType.Error);
                    AddInfoLog($"No response exception! Result is empty [{result}]");
                    StopTimer(stopwatch);
                    _internalFaultyMessageCount++;
                    retry = true;
                    DebugUtils.DebugLogMsg($"Retrying in {breakTime} seconds...", DebugUtils.DebugType.Error);
                    yield return new WaitForSeconds(breakTime);
                    breakTime *= 1.25f;
                }
                else
                {
                    StopTimer(stopwatch);
                    retry = false;
                }
            } while (retry && --maxAttempts >= 0);
            AddDataLog($"attempts,{maxAttempts - 5}");

            DebugUtils.DebugLogMsg(result, DebugUtils.DebugType.Temporary);

            var jsonResult = Sanitizer.ExtractJson(result);
            DebugUtils.DebugLogMsg(jsonResult, DebugUtils.DebugType.System);

            var actions = new LlmAction();
            try
            {
                actions = JsonConvert.DeserializeObject<LlmAction>(jsonResult);
            }
            catch (Exception e)
            {
                DebugUtils.DebugLogMsg($"Exception {e.Message}.", DebugUtils.DebugType.Error);
                AddInfoLog($"Casting exception! {e.Message}");
                DebugUtils.DebugLogErrorMsg(e.Message);
                _internalFaultyMessageCount++;
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
                AddInfoLog($"Casting exception on trying to act! {e.Message}");
                DebugUtils.DebugLogErrorMsg(e.Message);
            }

            if (shouldMove)
            {
                _internalMovementAttemptCount++;
                yield return StartCoroutine(LlmMoveCoroutine(movement));
            }

            if (shouldAttack)
            {
                _internalAttackAttemptCount++;
                yield return StartCoroutine(LlmAttackCoroutine(attack));
            }

            if (shouldMoveAfterAttack)
            {
                _internalMovementAttemptCount++;
                yield return StartCoroutine(LlmMoveCoroutine(moveAfterAttack));
            }

            FinishAITurn();
            yield break;

            void StopTimer(Stopwatch stopwatch)
            {
                stopwatch.Stop();
                var timeText = $"Request response in {stopwatch.ElapsedMilliseconds} ms.";
                DebugUtils.DebugLogMsg(timeText,
                    DebugUtils.DebugType.System);
                AddTimeInfoToLog($"request:({stopwatch.ElapsedMilliseconds})");
            }
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
                AddInfoLog($"Failed to move to {moveToPosition}");
                finishedMoving = true;
                _internalWrongMovementCount++;
            }

            yield return new WaitUntil(() => finishedMoving);
        }

        private IEnumerator LlmAttackCoroutine(Vector2Int attackPosition)
        {
            while (TryToAct())
            {
                var hasValidTarget = GridManager.GetSingleton().CheckGridPosition(attackPosition, out var targetUnit);
                if (!hasValidTarget && targetUnit.ActorsCount() <= 0)
                {
                    AddInfoLog($"No valid target chosen");
                    _internalWrongAttackCount++;
                    continue;
                }

                var canAttack = GridManager.GetSingleton()
                    .CanAttackFrom(currentUnit.Index(), attackPosition, navalCannon.GetCannonSo);
                if (canAttack)
                {
                    DebugUtils.DebugLogMsg($"{name} attacks {targetUnit}!", DebugUtils.DebugType.System);
                    var damage = CalculateDamage();
                    kills = targetUnit.DamageActors(damage);
                    AddInfoLog($"Attacked succeeded at {targetUnit}. Kill count = {kills}.");
                    yield return new WaitForSeconds(1.5f);
                }
                else
                {
                    var cannotReachMsg = $"Cannot reach target at {targetUnit}.";
                    DebugUtils.DebugLogMsg(cannotReachMsg, DebugUtils.DebugType.Error);
                    AddInfoLog(cannotReachMsg);
                    _internalWrongAttackCount++;
                }
            }

            yield return null;
        }

        protected override void FinishAITurn()
        {
            AddInfoLog("Finish turn");
            base.FinishAITurn();
        }

        protected override void DestroyActor()
        {
            LogFinalInformation();
            base.DestroyActor();
        }

        public void LogFinalInformation()
        {
            AddDataLog("_internalWrongMovementCount,_internalWrongAttackCount,_internalTotalRequestCount,_internalMovementAttemptCount,_internalAttackAttemptCount,_internalAttackAttemptCount, _internalFaultyMessageCount, kills");
            AddDataLog($"{_internalWrongMovementCount},{_internalWrongAttackCount},{_internalTotalRequestCount},{_internalMovementAttemptCount},{_internalAttackAttemptCount},{_internalAttackAttemptCount},{_internalFaultyMessageCount}, {kills}");
        }

        public string GetLlmInfo()
        {
            return $"{llmCaller.GetLlmType().ToString()}-{basePrompt.name}";
        }

        private void AddInfoLog(string info)
        {
            LevelController.GetSingleton().GetLogger().AddLine($"# {info} [{name}]");
        }
        
        private void AddDataLog(string data)
        {
            LevelController.GetSingleton().GetLogger().AddLine($"@ [{data}],[{name}]");
        }

        private void AddTimeInfoToLog(string timeInfo)
        {
            LevelController.GetSingleton().GetLogger().AddLine($"& [{timeInfo}],[{name}]");
        }
    }
}