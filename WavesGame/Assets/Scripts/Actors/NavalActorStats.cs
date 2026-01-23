/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 * 
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System;

namespace Actors
{
    [Serializable]
    public struct NavalActorStats
    {
        public StatValuePair strength; //Offensive
        public StatValuePair speed; //Movement and Number of Movements per Turn
        public StatValuePair stability; //Constitution against waves
        public StatValuePair sight; //Awareness
        public StatValuePair sturdiness; //Constitution against enemies
        public StatValuePair spirit; //Morale - number of actions per turn

        public void SetStats(int strengthValue, int speedValue, int stabilityValue, int sightValue, int sturdinessValue,
            int spiritValue)
        {
            strength.Two = strengthValue;
            speed.Two = speedValue;
            stability.Two = stabilityValue;
            sight.Two = sightValue;
            sturdiness.Two = sturdinessValue;
            spirit.Two = spiritValue;
        }

        public override string ToString()
        {
            return $"{strength}, {speed}, {stability}, {sight}, {sturdiness}, {spirit}";
        }
    }
}