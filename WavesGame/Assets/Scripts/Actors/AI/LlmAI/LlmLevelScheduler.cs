/*
 * Copyright (c) 2026 Yvens R Serpa [https://github.com/YvensFaos/]
 *
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System.Collections.Generic;
using System.Linq;
using Core;
using FALLA;
using Grid;
using NaughtyAttributes;
using UnityEngine;
using UUtils;

namespace Actors.AI.LlmAI
{
    public class LlmLevelScheduler : StrongSingleton<LlmLevelScheduler>
    {
        [SerializeField, ReadOnly] private List<LlmCallerObject> callers;
        [SerializeField] private List<LlmScheduleSo> schedules;
        [SerializeField, ReadOnly] private LlmScheduleSo currentSchedule;
        private int _internalCounter;

        protected override void Awake()
        {
            base.Awake();
            if (MarkedToDie) return;
            schedules.ForEach(schedule => schedule.Initialize());
        }

        public void BeginNewLevel()
        {
            callers = FindObjectsByType<LlmCallerObject>(FindObjectsSortMode.None).ToList();
        }

        public bool SetupLevel(List<GridActor> levelActors)
        {
            if (_internalCounter >= schedules.Count)
            {
                DebugUtils.DebugLogMsg($"All schedules done, current -> {_internalCounter}.", DebugUtils.DebugType.System);
                return false;
            }
            
            currentSchedule = schedules[_internalCounter];
            var llmActors = levelActors.Select(actor =>
            {
                if (actor is LlmAINavalShip llm)
                {
                    return llm;
                }

                return null;
            }).ToList().FindAll(actor => actor != null);

            var aiFactions = currentSchedule.GetFactions();
            foreach (var aiFaction in aiFactions)
            {
                var factionLlmActors = llmActors.FindAll(actor => actor.GetFaction().Equals(aiFaction));
                var pair = currentSchedule.GetFactionPair(aiFaction);
                pair.SetCaller(callers);

                foreach (var llmAINavalShip in factionLlmActors)
                {
                    llmAINavalShip.SetCaller(pair.Caller);
                    llmAINavalShip.UpdateName();
                }
            }

            return true;
        }

        public void FinishLevel(LevelGoal levelGoal)
        {
            DebugUtils.DebugLogMsg($"Finished level, current -> {_internalCounter}.", DebugUtils.DebugType.System);

            currentSchedule = schedules[_internalCounter];
            if (currentSchedule.Use())
            {
                _internalCounter++;
            }

            var winnerFaction = levelGoal.GetWinnerFaction();
            if (winnerFaction != null)
            {
                DebugUtils.DebugLogMsg($"Finished level, winner -> {winnerFaction}.", DebugUtils.DebugType.System);
                var winnerPair = currentSchedule.GetFactionPair(winnerFaction);
                LevelController.GetSingleton().AddInfoLog($"LLM {winnerPair.Caller} won.", "LevelGoal");    
            }
            else
            {
                DebugUtils.DebugLogMsg($"Finished level, DRAW!", DebugUtils.DebugType.System);
                LevelController.GetSingleton().AddInfoLog($"DRAW!", "LevelGoal");
            }
            
            DelayHelper.Delay(this, 5.0f, SceneLoader.ResetCurrentScene);
        }

        [Button("Shuffle Schedules")]
        public void ShuffleSchedule()
        {
            RandomHelper<LlmScheduleSo>.ShuffleList(ref schedules);
        }
    }
}