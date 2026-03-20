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
    public enum WavesRecordEntryType
    {
        Movement, Attack, Damage, Death
    }
    
    public static class WavesRecordEntryTypeTypeExtensions
    {
        public static string WavesRecordEntryTypeToString(WavesRecordEntryType type)
        {
            return type switch
            {
                WavesRecordEntryType.Movement => "MOVE",
                WavesRecordEntryType.Attack => "ATTK",
                WavesRecordEntryType.Damage => "DAMG",
                WavesRecordEntryType.Death => "DEAD",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}