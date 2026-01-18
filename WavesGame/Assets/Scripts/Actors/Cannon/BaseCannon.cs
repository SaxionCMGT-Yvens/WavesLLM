/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 * 
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using UnityEngine;
using UUtils;

namespace Actors.Cannon
{
    public class BaseCannon : MonoBehaviour
    {
        [SerializeField]
        private CannonSo cannonData;

        public CannonSo GetCannonSo => cannonData;

        public int CalculateDamage()
        {
            return cannonData.damage + DiceHelper.RollDiceFromString(cannonData.damageDie);
        }

        public override string ToString()
        {
            return $"{name} - {cannonData}";
        }
    }
}
