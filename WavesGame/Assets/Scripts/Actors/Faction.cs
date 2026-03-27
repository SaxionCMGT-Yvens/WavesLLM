using UnityEngine;

namespace Actors
{
    [CreateAssetMenu(fileName = "New Faction", menuName = "Waves/Faction", order = 1)]
    public class Faction : ScriptableObject
    {
        public override string ToString()
        {
            return name;
        }
    }
}