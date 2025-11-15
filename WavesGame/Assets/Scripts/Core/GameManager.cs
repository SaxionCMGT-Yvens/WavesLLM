using UnityEngine;
using UUtils;

namespace Core
{
    public class GameManager : StrongSingleton<GameManager>
    {
        [SerializeField]
        private GameManagerSettings gameManagerSettings;

        protected override void Awake()
        {
            base.Awake();
            DebugUtils.enabledDebugTypes = gameManagerSettings.enabledDebugTypes;
        }

        private void OnValidate()
        {
            DebugUtils.enabledDebugTypes = gameManagerSettings.enabledDebugTypes;
        }

        public GameManagerSettings GetSettings() => gameManagerSettings;
    }
}