using System;
using FALLA;
using UUtils;

namespace Actors.AI.LlmAI
{
    [Serializable]
    public class LlmModelPair : Pair<LlmType, string>
    {
        public LlmModelPair(LlmType one, string two) : base(one, two)
        {
        }

        public override string ToString()
        {
            return $"{One}-{(string.IsNullOrEmpty(Two) ? "default" : Two)}";
        }
    }
}