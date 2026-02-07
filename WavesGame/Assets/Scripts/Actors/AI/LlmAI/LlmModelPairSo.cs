using UnityEngine;

namespace Actors.AI.LlmAI
{
    [CreateAssetMenu(fileName = "Llm Model Pair", menuName = "Waves/LLM/Model Pair", order = 2)]
    public class LlmModelPairSo : ScriptableObject
    {
        public LlmModelPair modelPair;
    }
}