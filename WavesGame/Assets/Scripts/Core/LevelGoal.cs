/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 *
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System;
using System.Collections.Generic;
using Actors;
using Actors.AI;
using Grid;
using NaughtyAttributes;
using UnityEngine;
using UUtils;

namespace Core
{
    public enum LevelGoalType
    {
        DestroyAllTargets, DestroyAllEnemies, SurviveForTurns, DestroySpecificEnemy, AIWars, Custom
    }

    [Serializable]
    public class AIShipFactionPair : Pair<NavalShip, AIFaction>
    {
        public AIShipFactionPair(NavalShip one, AIFaction two) : base(one, two)
        { }
    }
    
    public class LevelGoal : MonoBehaviour
    {
        public LevelGoalType type;
        [SerializeField] private NavalActor destroyTarget;
        [SerializeField] private int surviveForTurns;
        [SerializeField, ReadOnly] private List<NavalTarget> levelTargets;
        [SerializeField, ReadOnly] private List<NavalShip> levelShips;
        [SerializeField, ReadOnly] private List<NavalShip> playerLevelShips;
        [SerializeField, ReadOnly] private List<NavalShip> enemyLevelShips;
        [SerializeField, ReadOnly] private List<AIShipFactionPair> enemyFactionShips;
        private Dictionary<AIFaction, int> _availableFactions;
        private int _survivedTurns;

        private void Awake()
        {
            _availableFactions = new Dictionary<AIFaction, int>();
        }

        public void Initialize(List<GridActor> levelActors)
        {
            levelActors.ForEach(actor =>
            {
                switch (actor)
                {
                    case NavalTarget target: levelTargets.Add(target); break;
                    case NavalShip navalShip:
                    {
                        levelShips.Add(navalShip);
                        if (navalShip.NavalType == NavalActorType.Player)
                        {
                            playerLevelShips.Add(navalShip);
                        }
                        else
                        {
                            enemyLevelShips.Add(navalShip);
                            if (navalShip is AINavalShip aiNavalShip)
                            {
                                var faction = aiNavalShip.GetFaction();
                                enemyFactionShips.Add(new AIShipFactionPair(aiNavalShip, faction));
                                if (!_availableFactions.TryAdd(faction, 1))
                                {
                                    _availableFactions[faction]++;
                                }
                            }
                        }
                    }
                        break;
                }
            });
        }
        
        public bool CheckGoalActor(NavalTarget navalTarget)
        {
            levelTargets.Remove(navalTarget);
            levelTargets.RemoveAll(target => target == null);
            return CheckGoal();
        }

        public bool CheckGoalActor(NavalShip navalShip)
        {
            if (navalShip.NavalType == NavalActorType.Player)
            {
                playerLevelShips.Remove(navalShip);
                playerLevelShips.RemoveAll(target => target == null);
            }
            else
            {
                enemyLevelShips.Remove(navalShip);
                enemyLevelShips.RemoveAll(target => target == null);

                if (navalShip is not AINavalShip aiNavalShip) return CheckGoal();
                var faction = aiNavalShip.GetFaction();
                _availableFactions[faction]--;
                DebugUtils.DebugLogMsg($"Naval Ship was an AI Ship {aiNavalShip.name} from the {faction} faction. Remaining: {_availableFactions[faction]}.", DebugUtils.DebugType.System);
            }
            return CheckGoal();
        }

        public bool CheckGoal()
        {
            switch (type)
            {
                case LevelGoalType.DestroyAllTargets:
                    return levelTargets.Count <= 0;
                case LevelGoalType.DestroyAllEnemies:
                    return enemyLevelShips.Count <= 0;
                case LevelGoalType.SurviveForTurns:
                    return _survivedTurns >= surviveForTurns;
                case LevelGoalType.DestroySpecificEnemy:
                    return destroyTarget == null || destroyTarget.GetCurrentHealth() <= 0;
                case LevelGoalType.Custom:
                    //TODO
                    break;
                case LevelGoalType.AIWars:
                {
                    var enumerator = _availableFactions.GetEnumerator();
                    var alive = 0;
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;
                        if (current.Value > 0)
                        {
                            alive++;
                        }
                    }
                    //Only one survived, then it won!
                    enumerator.Dispose();
                    DebugUtils.DebugLogMsg($"Factions remaining {alive}", DebugUtils.DebugType.System);
                    return alive == 1;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return false;
        }

        public string GetLevelMessage()
        {
            return type switch
            {
                LevelGoalType.DestroyAllTargets => "Destroy All Targets",
                LevelGoalType.DestroyAllEnemies => "Destroy All Enemies",
                LevelGoalType.SurviveForTurns => $"Survive for {surviveForTurns} Turns",
                LevelGoalType.DestroySpecificEnemy => $"Destroy {destroyTarget.name}",
                LevelGoalType.Custom => $"Custom Goal",
                LevelGoalType.AIWars => $"AI Wars",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void SurvivedTurn()
        {
            _survivedTurns++;
        }

        public bool CheckGameOver()
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (type)
            {
                case LevelGoalType.AIWars:
                    return false;
                case LevelGoalType.Custom:
                    //TODO change this in the future
                    return false;
                default:
                    return playerLevelShips.Count <= 0;
            }
        }
        
        //TODO for the custom type, create a sort of prefab with script checker.
    }
}