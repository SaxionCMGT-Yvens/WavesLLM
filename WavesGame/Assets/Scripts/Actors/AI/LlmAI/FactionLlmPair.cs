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

        public FactionLlmPair(AIFaction one, LlmModelPairSo two) : base(one, two)
        {
        }

        public void SetCaller(List<LlmCallerObject> callers)
        {
            caller = callers.Find(call => call.GetLlmType().Equals(Two.modelPair.One));
            var specificModel = Two.modelPair.Two;
            if (!string.IsNullOrEmpty(specificModel))
            {
                Caller.LoadModel(specificModel);
            }
        }

        public LlmCallerObject Caller => caller;

        public override string ToString()
        {
            return Two.modelPair.ToString();
        }
    }
}