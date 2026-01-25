using System.Collections;
using Core;
using NaughtyAttributes;
using UnityEngine;
using UUtils;

namespace Actors.AI
{
    public abstract class AIBaseShip : NavalShip
    {
        [SerializeField] private AIFaction faction;

        [Header("Score")] [SerializeField, ReadOnly]
        protected int kills;

        protected override void Awake()
        {
            base.Awake();
            kills = 0;
        }

        public override void StartTurn()
        {
            base.StartTurn();
            //TODO change
            CursorController.GetSingleton().ToggleActive(true);
            StartCoroutine(TurnAI());
        }

        public override void EndTurn()
        {
            base.EndTurn();
            CursorController.GetSingleton().ToggleActive(true);
        }

        protected void FinishAITurn()
        {
            LevelController.GetSingleton().EndTurnForCurrentActor();
            DebugUtils.DebugLogMsg($"{name} has finished its turn.", DebugUtils.DebugType.System);
        }

        protected abstract IEnumerator TurnAI();

        public AIFaction GetFaction() => faction;
        public int GetKills() => kills;

        public override string ToString()
        {
            return $"{base.ToString()}; faction={faction}; kills={GetKills()}";
        }

        public string ToLlmString()
        {
            return $"[{name}]; faction={faction}; currentHealth={GetCurrentHealth()}."; 
        }
    }
}