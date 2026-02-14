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
        [SerializeField, ReadOnly] private int internalCounter;
        [SerializeField, ReadOnly] private int internalRepetition;

        public int InternalRepetition => internalRepetition;

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
            if (internalCounter >= schedules.Count)
            {
                DebugUtils.DebugLogMsg($"Finish! All schedules done, current -> {internalCounter}.", DebugUtils.DebugType.System);
                return false;
            }
            
            currentSchedule = schedules[internalCounter];
            internalRepetition = currentSchedule.InternalRepetitionsCount;
            
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
            
            var customFactions = currentSchedule.factionPairs.FindAll(pair => pair.Two.modelPair.One == LlmType.Custom);
            foreach (var customFaction in customFactions)
            {
                var faction = customFaction.One;
                var llmFactionShips = llmActors.FindAll(actor => actor.GetFaction().Equals(faction));

                foreach (var llmFactionShip in llmFactionShips)
                {
                    var newAiBaseShip = Instantiate(customFaction.AIBaseShipPrefab, llmFactionShip.transform.position, llmFactionShip.transform.rotation);
                    //TODO check if this works for the grid
                    newAiBaseShip.SetUnit(llmFactionShip.GetUnit());
                    newAiBaseShip.SetInitiative(llmFactionShip.OverrideInitiative);
                    Destroy(llmFactionShip.gameObject);
                }
            }

            return true;
        }

        public void FinishLevel(LevelGoal levelGoal)
        {
            DebugUtils.DebugLogMsg($"Finished level, current -> {internalCounter}.", DebugUtils.DebugType.System);

            currentSchedule = schedules[internalCounter];
            if (currentSchedule.Use())
            {
                internalCounter++;
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