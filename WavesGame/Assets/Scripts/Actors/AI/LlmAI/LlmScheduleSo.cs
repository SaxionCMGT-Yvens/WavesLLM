using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UUtils;

namespace Actors.AI.LlmAI
{
    [CreateAssetMenu(fileName = "LlmSchedule", menuName = "Waves/LLM/Schedule", order = 1)]
    public class LlmScheduleSo : ScriptableObject
    {
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
}