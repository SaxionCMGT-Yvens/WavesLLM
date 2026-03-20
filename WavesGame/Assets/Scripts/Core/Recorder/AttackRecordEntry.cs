/*
 * Copyright (c) 2026 Yvens R Serpa [https://github.com/YvensFaos/]
 *
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System;
using UnityEngine;

namespace Core.Recorder
{
    [Serializable]
    public class AttackRecordEntry : ActorRecordEntry
    {
        private Vector2Int _attackPosition;
        private float _damage;

        public AttackRecordEntry(string actorId, Vector2Int attackPosition, float damage) : base(actorId)
        {
            _attackPosition = attackPosition;
            _damage = damage;
            type = WavesRecordEntryType.Attack;
        }

        protected override string Content()
        {
            return $";{_attackPosition};{_damage}";
        }

        public Vector2Int AttackPosition => _attackPosition;
        public float Damage => _damage;
    }
}