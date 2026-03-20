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
    public class MovementRecordEntry : ActorRecordEntry
    {
        private Vector2Int _moveTo;
        
        public MovementRecordEntry(string actorId, Vector2Int moveTo) : base(actorId)
        {
            _moveTo = moveTo;
            type = WavesRecordEntryType.Movement;
        }
        
        
        protected override string Content()
        {
            return $";{_moveTo}";
        }

        public Vector2Int MoveTo => _moveTo;
    }
}