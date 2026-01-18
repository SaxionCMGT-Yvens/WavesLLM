using UnityEngine;

namespace Actors.AI
{
    [CreateAssetMenu(fileName = "New AIFaction", menuName = "Waves/AI/AI Faction", order = 1)]
    public class AIFaction : ScriptableObject
    {
        public override string ToString()
        {
            return name;
        }
    }
}