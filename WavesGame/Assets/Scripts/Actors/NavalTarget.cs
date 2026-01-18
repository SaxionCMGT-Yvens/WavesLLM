/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 * 
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using Core;
using UnityEngine;
using UUtils;

namespace Actors
{
    public class NavalTarget : NavalActor
    {
        [SerializeField]
        private NavalActorStats stats;

        //TODO move this repeated code from Target and Ship to NavalActor with sturdiness as a parameter.
        public override bool TakeDamage(int damage)
        {
            var damageTaken = damage - stats.sturdiness.Two;
            DebugUtils.DebugLogMsg($"{name} attacked with {damage}. Sturdiness is {stats.sturdiness}. Damage taken was {damageTaken}.", DebugUtils.DebugType.Temporary);
            return base.TakeDamage(damageTaken);
        }

        protected override void NotifyLevelController()
        {
            LevelController.GetSingleton().NotifyDestroyedActor(this);
        }

        public NavalActorStats GetStats() => stats;

        public override string ToString()
        {
            return $" {base.ToString()}; stats={stats}";
        }
    }
}