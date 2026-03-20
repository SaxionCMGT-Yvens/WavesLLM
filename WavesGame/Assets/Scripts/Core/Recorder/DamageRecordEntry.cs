/*
 * Copyright (c) 2026 Yvens R Serpa [https://github.com/YvensFaos/]
 *
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System;

namespace Core.Recorder
{
    [Serializable]
    public class DamageRecordEntry : ActorRecordEntry
    {
        private float _damage;

        public DamageRecordEntry(string actorId, float damage) : base(actorId)
        {
            _damage = damage;
            type = WavesRecordEntryType.Damage;
        }

        protected override string Content()
        {
            return $";{_damage}";
        }

        public float Damage => _damage;
    }
}