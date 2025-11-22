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
        public override void TakeDamage(int damage)
        {
            var damageTaken = damage - stats.sturdiness.Two;
            DebugUtils.DebugLogMsg($"{name} attacked with {damage}. Sturdiness is {stats.sturdiness}. Damage taken was {damageTaken}.", DebugUtils.DebugType.Temporary);
            base.TakeDamage(damageTaken);
        }

        protected override void NotifyLevelController()
        {
            LevelController.GetSingleton().NotifyDestroyedActor(this);
        }

        public NavalActorStats GetStats() => stats;
    }
}