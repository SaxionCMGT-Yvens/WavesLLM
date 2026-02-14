using System;
using System.Collections.Generic;
using FALLA;
using NaughtyAttributes;
using UnityEngine;
using UUtils;

namespace Actors.AI.LlmAI
{
    [Serializable]
    public class FactionLlmPair : Pair<AIFaction, LlmModelPairSo>
    {
        [SerializeField, ReadOnly] private LlmCallerObject caller;
        [SerializeField] private bool matchModel;
        
        [Header("For Custom LLM Type and Regular AIs")]
        [SerializeField] private AIBaseShip aiBaseShipPrefab;

        public FactionLlmPair(AIFaction one, LlmModelPairSo two) : base(one, two)
        {
        }

        public void SetCaller(List<LlmCallerObject> callers)
        {
            var llmType = Two.modelPair.One;
            if (llmType == LlmType.Custom) return;
            var llmModel = Two.modelPair.Two;
            caller = callers.Find(call =>
            {
                if (matchModel)
                {
                    return call.GetLlmType().Equals(llmType) && call.GetLlmModel().Equals(llmModel);
                }
                return call.GetLlmType().Equals(llmType);
            });
            if (!string.IsNullOrEmpty(llmModel))
            {
                Caller.LoadModel(llmModel);
            }
        }

        public LlmCallerObject Caller => caller;

        public AIBaseShip AIBaseShipPrefab => aiBaseShipPrefab;

        public override string ToString()
        {
            return Two.modelPair.ToString();
        }
    }
}