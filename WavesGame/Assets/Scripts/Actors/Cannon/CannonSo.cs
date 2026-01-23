/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 * 
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using Grid;
using UnityEngine;

namespace Actors.Cannon
{
    [CreateAssetMenu(fileName = "New Cannon", menuName = "Waves/Cannon", order = 1)]
    public class CannonSo : ScriptableObject
    {
        public int area;
        public int deadZone;
        public int damage;
        public string damageDie;
        public GridMoveType targetAreaType;

        public override string ToString()
        {
            return $"[Cannon = {name}; area = {area}; dead zone = {deadZone}; damage = {damage}; damage die = {damageDie}; targetAreaType = {targetAreaType.ToString()}]";
        }
    }
}