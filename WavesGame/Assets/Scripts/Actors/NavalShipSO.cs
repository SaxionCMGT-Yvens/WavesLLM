using Core;
using NaughtyAttributes;
using UnityEngine;
using UUtils;

namespace Actors
{
    [CreateAssetMenu(fileName = "New Naval Ship", menuName = "Waves/Naval Ship", order = 0)]
    public class NavalShipSo : ScriptableObject
    {
        [ShowAssetPreview] public Sprite shipSprite;
        [Header("Stats")] public NavalActorStats stats;
        public string initiativeDie;

        public int RollInitiative()
        {
            return stats.speed.Two + DiceHelper.RollDiceFromString(initiativeDie);
        }

        /// <summary>
        /// Checks if this NavalShip will be moved by a wave or not.
        /// Rolls a D10 and checks if the result is smaller than the Stability attribute.
        /// </summary>
        /// <returns>True if the NavalShip remains steady against the wave force.</returns>
        public bool ResistWave()
        {
            if (GameManager.GetSingleton().GetSettings().alwaysHitByWaves) return false;
            if (GameManager.GetSingleton().GetSettings().alwaysIgnoreWaves) return true;
            
            var resist = DiceHelper.RollDie(10);
            return resist <= stats.stability.Two;
        }
        
        //TODO add a validation for the initiativeDie
    }
}