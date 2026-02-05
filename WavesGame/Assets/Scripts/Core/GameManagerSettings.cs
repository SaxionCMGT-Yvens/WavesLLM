/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 *
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using NaughtyAttributes;
using UnityEngine;
using UUtils;

namespace Core
{
    [CreateAssetMenu(fileName = "Game Manager Settings", menuName = "Waves/Game Manager Settings", order = 10)]
    public class GameManagerSettings : ScriptableObject
    {
        public DebugUtils.DebugType enabledDebugTypes = DebugUtils.DebugType.Regular | DebugUtils.DebugType.System |
                                                        DebugUtils.DebugType.Warning | DebugUtils.DebugType.Error;

        public bool logToFile = false;
        [ShowIf("logToFile")] public string logName = "wavesGame";
        public bool debugCursorInformation = false;
        public bool alwaysIgnoreWaves = false;
        public bool alwaysHitByWaves = false;
    }
}