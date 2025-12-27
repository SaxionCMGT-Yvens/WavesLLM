/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 * 
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System.Collections.Generic;
using Actors.Cannon;
using Grid;
using UnityEngine;
using UUtils;

namespace Actors.AI
{
    public class AIBrain
    {
        private readonly AINavalShip _aiNavalShip;
        private readonly CannonSo _cannonData;

        public AIBrain(AINavalShip aiNavalShip, CannonSo cannonData)
        {
            DebugUtils.DebugLogMsg("Initializing AIBrain", DebugUtils.DebugType.Temporary);
            _aiNavalShip = aiNavalShip;
            _cannonData = cannonData;
        }

        public bool CalculateMovement(Vector2Int position, int stepsAvailable, out AIGridUnitUtility chosenAction)
        {
            var walkableUnits = GridManager.GetSingleton().GetGridUnitsInRadiusManhattan(position, stepsAvailable);
            var utilities = new List<AIGridUnitUtility>();
            var genes = _aiNavalShip.GetGenesData();

            //First calculate all possible movements
            foreach (var unit in walkableUnits)
            {
                var gridUnitUtility = new AIGridUnitUtility(unit);
                //Calculate the utility of moving to a given position
                var movementUtility = AIGridUnitUtility.CalculateUtilityToMoveToGridUnit(_aiNavalShip, unit);
                var awarenessUtility = 0.0f;
                var attackUtility = 0.0f;
                    
                //Then calculate the utility of the surroundings of the given position
                var awarenessRadius = Mathf.FloorToInt(genes.awareness);
                if (awarenessRadius >= 1)
                {
                    var awarenessUnits = GridManager.GetSingleton()
                        .GetGridUnitsInRadiusManhattan(unit.Index(), awarenessRadius);
                    //Remove self position
                    awarenessUnits.Remove(unit);
                    awarenessUnits.ForEach(awarenessUnit =>
                    {
                        awarenessUtility += AIGridUnitUtility.CalculateProximityUtility(_aiNavalShip, awarenessUnit);
                    });
                }
                
                //Finally, calculate the utilities of attacking from this position
                var attackableFromUnit = GridManager.GetSingleton().GetGridUnitsForMoveType(_cannonData.targetAreaType,
                    unit.Index(), _cannonData.area, _cannonData.deadZone);
                if (attackableFromUnit.Count >= 0)
                {
                    //Remove self position
                    attackableFromUnit.Remove(unit);
                    attackableFromUnit.ForEach(attackUnit =>
                    {
                        attackUtility += AIGridUnitUtility.CalculatePossibleAttackUtility(_aiNavalShip, attackUnit);
                    });
                }
                
                gridUnitUtility.Utility = movementUtility + awarenessUtility + attackUtility;
                utilities.Add(gridUnitUtility);
            }

            chosenAction = null;
            if (utilities.Count == 0) return false;
            utilities.Sort();
            var possibleActionsCount = Mathf.Min(utilities.Count, genes.possibleActionsCount);
            var possibleActions = utilities.GetRange(0, possibleActionsCount);
            chosenAction = RandomHelper<AIGridUnitUtility>.GetRandomFromListWithIndex(possibleActions, out var index);
            DebugUtilityChoices(chosenAction, index, utilities);
            return true;
        }

        public bool CalculateAction(Vector2Int position, out AIGridUnitUtility chosenAction)
        {
            var attackableFromUnit = GridManager.GetSingleton().GetGridUnitsForMoveType(_cannonData.targetAreaType,
                position, _cannonData.area, _cannonData.deadZone);
            chosenAction = null;
            
            if(attackableFromUnit == null || attackableFromUnit.Count == 0) return false;

            var utilities = new List<AIGridUnitUtility>();
            foreach (var unit in attackableFromUnit)
            {
                var gridUnitUtility = new AIGridUnitUtility(unit);
                var utility = AIGridUnitUtility.CalculateAttackUtility(_aiNavalShip, unit);
                if (Mathf.Approximately(utility, float.MinValue)) continue;
                gridUnitUtility.Utility = utility;
                utilities.Add(gridUnitUtility);
            }
            if (utilities.Count == 0) return false;
            utilities.Sort();
            var possibleActionsCount = Mathf.Min(utilities.Count, _aiNavalShip.GetGenesData().possibleActionsCount);
            var possibleActions = utilities.GetRange(0, possibleActionsCount);
            chosenAction = RandomHelper<AIGridUnitUtility>.GetRandomFromListWithIndex(possibleActions, out var index);
            DebugUtilityChoices(chosenAction, index, utilities);
            return true;
        }

        public static GridUnit GenerateRandomMovement(Vector2Int position, int stepsAvailable)
        {
            var walkableUnits = GridManager.GetSingleton().GetGridUnitsInRadiusManhattan(position, stepsAvailable);
            return RandomHelper<GridUnit>.GetRandomFromList(walkableUnits);
        }
        
        
        private static void DebugUtilityChoices(AIGridUnitUtility chosenAction, int index, List<AIGridUnitUtility> utilities)
        {
            //TODO block this when building
            
            
            DebugUtils.DebugLogMsg($"Action {index}/{utilities.Count}: {chosenAction} chosen.", DebugUtils.DebugType.Regular);
            for (var i = 0; i < Mathf.Min(5, utilities.Count); i++)
            {
                DebugUtils.DebugLogMsg($"Utils => {i} {utilities[i]}", DebugUtils.DebugType.Verbose);
            }
        }
    }
}