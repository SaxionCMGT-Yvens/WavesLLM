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

        public int InternalRepetitionsCount => internalRepetitionsCount;

        [Button("Initialize")]
        public void Initialize()
        {
            internalRepetitionsCount = repetitions;
        }

        public bool Use()
        {
            internalRepetitionsCount = Mathf.Max(0, InternalRepetitionsCount - 1);
            DebugUtils.DebugLogMsg(
                $"Update Schedule repetition, _internalRepetitionsCount -> {InternalRepetitionsCount}.",
                DebugUtils.DebugType.System);
            return InternalRepetitionsCount <= 0;
        }

        public List<AIFaction> GetFactions()
        {
            return factionPairs.Select(pair => pair.One).ToList();
        }

        public FactionLlmPair GetFactionPair(AIFaction faction)
        {
            return factionPairs.Find(pair => pair.One.Equals(faction));
        }

        public override string ToString()
        {
            var str = "";
            factionPairs.ForEach(pair => str += pair + "=");
            if (str.Length > 0)
            {
                str = str[..^1];
            }
            return str;
        }
    }
}