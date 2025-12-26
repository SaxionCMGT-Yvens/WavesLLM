/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 * 
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System.Collections;
using Core;
using UnityEngine;
using UUtils;

namespace Actors.AI
{
    public class AINavalShip : NavalShip
    {
        public override void StartTurn()
        {
            base.StartTurn();
            CursorController.GetSingleton().ToggleActive(false);
            StartCoroutine(TurnAI());
        }

        public override void EndTurn()
        {
            base.EndTurn();
            CursorController.GetSingleton().ToggleActive(true);
        }

        private void FinishAITurn()
        {
            LevelController.GetSingleton().EndTurnForCurrentActor();
            DebugUtils.DebugLogMsg($"{name} has finished its turn.", DebugUtils.DebugType.System);
        }

        private IEnumerator TurnAI()
        {
            yield return new WaitForSeconds(2.0f);
            FinishAITurn();
        }
    }
}