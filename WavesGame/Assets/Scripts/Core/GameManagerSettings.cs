using UnityEngine;
using UUtils;

namespace Core
{
    [CreateAssetMenu(fileName = "Game Manager Settings", menuName = "Waves/Game Manager Settings", order = 10)]
    public class GameManagerSettings : ScriptableObject
    {
        public DebugUtils.DebugType enabledDebugTypes = DebugUtils.DebugType.Regular | DebugUtils.DebugType.System |
                                                         DebugUtils.DebugType.Warning | DebugUtils.DebugType.Error;

        public bool debugCursorInformation = false;
        public bool alwaysIgnoreWaves = false;
        public bool alwaysHitByWaves = false;
    }
}