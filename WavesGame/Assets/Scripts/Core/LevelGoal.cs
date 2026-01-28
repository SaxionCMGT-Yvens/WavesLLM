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
using Actors.AI.LlmAI;
using Grid;
using NaughtyAttributes;
using UnityEngine;
using UUtils;

namespace Core
{
    public enum LevelGoalType
    {
        DestroyAllTargets,
        DestroyAllEnemies,
        SurviveForTurns,
        DestroySpecificEnemy,
        AIWars,
        Custom
    }

    [Serializable]
    public class AIShipFactionPair : Pair<NavalShip, AIFaction>
    {
        public AIShipFactionPair(NavalShip one, AIFaction two) : base(one, two)
        {
        }
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
                            switch (navalShip)
                            {
                                case AINavalShip aiNavalShip:
                                {
                                    IncreaseFactionCount(aiNavalShip);
                                    break;
                                }
                                case LlmAINavalShip llmNavalShip:
                                {
                                    IncreaseFactionCount(llmNavalShip);
                                    break;
                                }
                            }
                        }
                    }
                        break;
                }
            });
        }

        private void IncreaseFactionCount(AIBaseShip aiShip)
        {
            var faction = aiShip.GetFaction();
            enemyFactionShips.Add(new AIShipFactionPair(aiShip, faction));
            if (!_availableFactions.TryAdd(faction, 1))
            {
                _availableFactions[faction]++;
            }
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

                switch (navalShip)
                {
                    case LlmAINavalShip llmNavalShip:
                    {
                        UpdateFactionCount(llmNavalShip);
                        break;
                    }
                    case AINavalShip aiNavalShip:
                    {
                        UpdateFactionCount(aiNavalShip);
                        break;
                    }
                    default:
                        return CheckGoal();
                }
            }

            return CheckGoal();

            void UpdateFactionCount(AIBaseShip aiBaseShip)
            {
                var faction = aiBaseShip.GetFaction();
                _availableFactions[faction]--;
                DebugUtils.DebugLogMsg(
                    $"Naval Ship was an AI Ship {aiBaseShip.name} from the {faction} faction. Remaining: {_availableFactions[faction]}.",
                    DebugUtils.DebugType.System);
            }
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
                    AIFaction aliveFaction = null;
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;
                        if (current.Value <= 0) continue;
                        aliveFaction = current.Key;
                        alive++;
                    }

                    //Only one survived, then it won!
                    enumerator.Dispose();
                    DebugUtils.DebugLogMsg($"Factions remaining {alive}", DebugUtils.DebugType.System);
                    var endLevel = alive == 1;
                    if (!endLevel) return false;
                    
                    LevelController.GetSingleton().GetLogger().AddLine($"Faction {aliveFaction} won.");
                    LevelController.GetSingleton().GetLogger().AddLine($"Logging remaining ships. Count: {enemyFactionShips.Count}.");
                    
                    // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                    foreach (var aiShipPair in enemyFactionShips)
                    {
                        var aiShip = aiShipPair.One;
                        if (aiShip != null && aiShip is LlmAINavalShip llmAINavalShip)
                        {
                            llmAINavalShip.LogFinalInformation();
                        }
                    }
                    return true;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
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

        public string GetLevelMessage()
        {
            var message = $"{type}";
            switch (type)
            {
                case LevelGoalType.DestroyAllTargets: message += "Destroy All Targets"; break;
                case LevelGoalType.DestroyAllEnemies:
                    message += "Destroy All Enemies";
                    break;
                case LevelGoalType.SurviveForTurns:
                    message += $"Survive For {surviveForTurns} Turns";
                    break;
                case LevelGoalType.DestroySpecificEnemy:
                    message += $"Destroy {destroyTarget.name}";
                    break;
                case LevelGoalType.AIWars:
                    message = "AI Wars = ";
                    var enumerator = _availableFactions.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        var llmInfo = "";
                        var aiShip = enemyFactionShips.Find(ship => ship.Two.Equals(enumerator.Current.Key));
                        if (aiShip is { One: LlmAINavalShip llmNavalShip })
                        {
                            llmInfo = $"[{llmNavalShip.GetLlmInfo()}]";
                        }

                        message += $"{enumerator.Current.Key}{llmInfo} x ";
                    }

                    enumerator.Dispose();
                    message = message[..^2];
                    break;
                case LevelGoalType.Custom:
                    message += $"Custom Goal";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return message;
        }

        //TODO for the custom type, create a sort of prefab with script checker.
    }
}