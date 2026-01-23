/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 * 
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System;
using UUtils;

namespace Actors
{
    [Serializable]
    public class StatValuePair : Pair<StatSO, int>
    {
        public StatValuePair(StatSO one, int two) : base(one, two)
        { }

        public override string ToString()
        {
            return $"{One.statName}: {Two}";
        }
    }
}