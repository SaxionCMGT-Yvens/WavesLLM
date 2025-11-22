using System;
using System.Collections.Generic;
using Actors;
using Grid;
using NaughtyAttributes;
using UnityEngine;

namespace Core
{
    public enum LevelGoalType
    {
        DestroyAllTargets, DestroyAllEnemies, SurviveForTurns, DestroySpecificEnemy, Custom
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
        private int _survivedTurns;

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
            return playerLevelShips.Count <= 0;
        }
        
        //TODO for the custom type, create a sort of prefab with script checker.
    }
}