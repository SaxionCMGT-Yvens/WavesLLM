/*
 * Copyright (c) 2026 Yvens R Serpa [https://github.com/YvensFaos/]
 *
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System;
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
    [Serializable]
    internal class LevelSchedule
    {
        public int orderId;
        public int repetitions;
        [SerializeField, ReadOnly]
        private int internalRepetitionsCount;
        public List<FactionLlmPair> factionPairs;

        public void Initialize()
        {
            internalRepetitionsCount = repetitions;
        }

        public bool Use()
        {
            internalRepetitionsCount = Mathf.Max(0, internalRepetitionsCount - 1);
            DebugUtils.DebugLogMsg(
                $"Update Schedule repetition, _internalRepetitionsCount -> {internalRepetitionsCount}.",
                DebugUtils.DebugType.System);
            return internalRepetitionsCount <= 0;
        }

        public List<AIFaction> GetFactions()
        {
            return factionPairs.Select(pair => pair.One).ToList();
        }

        public FactionLlmPair GetFactionPair(AIFaction faction)
        {
            return factionPairs.Find(pair => pair.One.Equals(faction));
        }
    }

    [Serializable]
    internal class FactionLlmPair : Pair<AIFaction, LlmType>
    {
        [SerializeField] private string specificModel;
        [SerializeField, ReadOnly] private LlmCallerObject caller;

        public FactionLlmPair(AIFaction one, LlmType two) : base(one, two)
        {
        }

        public void SetCaller(List<LlmCallerObject> callers)
        {
            caller = callers.Find(call => call.GetLlmType().Equals(Two));
            if (!string.IsNullOrEmpty(specificModel))
            {
                Caller.LoadModel(specificModel);
            }
        }

        public LlmCallerObject Caller => caller;
    }

    public class LlmLevelScheduler : StrongSingleton<LlmLevelScheduler>
    {
        [SerializeField, ReadOnly] private List<LlmCallerObject> callers;
        [SerializeField] private List<LevelSchedule> schedules;
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
            
            var currentSchedule = schedules[_internalCounter];
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

            var currentSchedule = schedules[_internalCounter];
            if (currentSchedule.Use())
            {
                _internalCounter++;
            }

            var winnerFaction = levelGoal.GetWinnerFaction();
            DebugUtils.DebugLogMsg($"Finished level, winner -> {winnerFaction}.", DebugUtils.DebugType.System);
            var winnerPair = currentSchedule.GetFactionPair(winnerFaction);
            LevelController.GetSingleton().AddInfoLog($"LLM {winnerPair.Caller} won.", "LevelGoal");
            DelayHelper.Delay(this, 1.5f, SceneLoader.ResetCurrentScene);
        }
    }
}