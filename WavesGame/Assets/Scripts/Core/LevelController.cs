/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 * 
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Actors;
using Grid;
using NaughtyAttributes;
using TMPro;
using UI;
using UnityEngine;
using UUtils;

namespace Core
{
    [Serializable]
    internal class LevelActorPair : Pair<NavalShip, bool>, IComparable<LevelActorPair>
    {
        public LevelActorPair(NavalShip one) : base(one, true)
        {
        }

        public static implicit operator bool(LevelActorPair pair) => pair.One != null && pair.Two;

        public int CompareTo(LevelActorPair other)
        {
            return other.One.Initiative.CompareTo(other.One.Initiative);
        }
    }

    public class LevelController : WeakSingleton<LevelController>
    {
        [Header("Data")] [SerializeField] private List<GridActor> levelActors;
        [SerializeField, ReadOnly] private List<NavalActor> levelNavalActors;
        private List<LevelActorPair> _levelActionableActor;
        [SerializeField, ReadOnly] private List<ActorTurnUI> actorTurnUIs;

        [Header("Level Specific")] [SerializeField]
        private LevelGoal levelGoal;
        [SerializeField, Scene]
        private string nextLevelName;

        [Header("References")] [SerializeField]
        private RectTransform actorTurnsHolder;

        [SerializeField] private ActorTurnUI actorTurnUIPrefab;
        [SerializeField] private EndLevelPanelUI endLevelPanelUI;
        [SerializeField] private TextMeshProUGUI levelGoalText;

        private Coroutine _levelCoroutine;
        private NavalActor _currentActor;
        private bool _endTurn;
        private bool _finishedLevel;

        private void Start()
        {
            endLevelPanelUI.gameObject.SetActive(false);
            _levelCoroutine = StartCoroutine(LevelCoroutine());
        }

        private IEnumerator LevelCoroutine()
        {
            //Wait for one frame for all elements to be initialized
            yield return null;

            //Initialize level goal elements
            levelGoal.Initialize(levelActors);
            levelGoalText.text = levelGoal.GetLevelMessage();

            //Roll initiatives and order turns
            _levelActionableActor.ForEach(actorPair => actorPair.One.RollInitiative());
            _levelActionableActor.Sort(((pairOne, pairTwo) => pairTwo.One.Initiative.CompareTo(pairOne.One.Initiative)));
            _levelActionableActor.ForEach(actorPair =>
            {
                DebugUtils.DebugLogMsg($"Creating actor UI {actorPair.One.gameObject.name} [{actorPair.One.Initiative}]", DebugUtils.DebugType.System);
                AddLevelActorToTurnBar(actorPair.One);
            });

            //Start level
            var enumerator = _levelActionableActor.GetEnumerator();
            var continueLevel = true;
            var victory = false;
            var gameOver = false;
            while (continueLevel)
            {
                //There are no actors left. Finish the level cycle.
                if (actorTurnUIs.Count == 0)
                {
                    continueLevel = false;
                    continue;
                }

                while (enumerator.MoveNext())
                {
                    //If the current is valid, then proceed with its turn.
                    if (!enumerator.Current) continue;
                    _currentActor = enumerator.Current?.One;
                    _endTurn = false;
                    if (_currentActor is NavalShip navalShip)
                    {
                        var turnUI = GetActorTurnUI(navalShip);
                        turnUI.ToggleAvailability(true);
                        navalShip.StartTurn();
                        //move the cursor to the ship
                        CursorController.GetSingleton().MoveToIndex(navalShip.GetUnit().Index());
                        
                        yield return new WaitUntil(() => _endTurn);
                        //Check if the naval ship was not destroyed during its own turn.
                        if (navalShip == null) continue;
                        navalShip.EndTurn();
                        
                        //TODO check if this is necessary of it maybe this has been destroyed already
                        if (enumerator.Current is { Two: true })
                        {
                            turnUI.ToggleAvailability(false);
                        }
                    }
                    else
                    {
                        //TODO create the logic for non-player _endTurn logic
                        yield return new WaitUntil(() => _endTurn);
                    }
                }

                enumerator.Dispose();
                //Finished going through all characters
                levelGoal.SurvivedTurn();
                victory = levelGoal.CheckGoal();
                gameOver = levelGoal.CheckGameOver();
                if (victory || gameOver)
                {
                    continueLevel = false;
                }
                else
                {
                    //If there are no more enumerators ahead, then start from the beginning.
                    enumerator = _levelActionableActor.GetEnumerator();
                }
            }

            enumerator.Dispose();
            if (gameOver)
            {
                victory = false;
            }

            FinishLevel(victory);
        }

        /// <summary>
        /// Allows the LevelController to continue.
        /// </summary>
        public void EndTurnForCurrentActor()
        {
            _endTurn = true;
        }

        public void AddLevelActor(GridActor actor)
        {
            levelActors.Add(actor);
            if (actor is not NavalActor navalActor) return;
            levelNavalActors.Add(navalActor);
            switch (navalActor.NavalType)
            {
                case NavalActorType.Player:
                case NavalActorType.Enemy:
                    if (navalActor is NavalShip navalShip)
                    {
                        _levelActionableActor ??= new List<LevelActorPair>();
                        _levelActionableActor.Add(new LevelActorPair(navalShip));
                    }

                    break;
                case NavalActorType.Collectable:
                case NavalActorType.Obstacle:
                case NavalActorType.Wave:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AddLevelActorToTurnBar(NavalShip navalShip)
        {
            var newActorTurnUI = Instantiate(actorTurnUIPrefab, actorTurnsHolder);
            newActorTurnUI.Initialize(navalShip);
            actorTurnUIs.Add(newActorTurnUI);
        }

        public bool IsCurrentActor(NavalActor navalActor)
        {
            return _currentActor.Equals(navalActor);
        }

        public void NotifyDestroyedActor(NavalActor navalActor)
        {
            //TODO logic for a generic actor being destroyed
            DebugUtils.DebugLogMsg($"Naval Actor {navalActor.name} notified Level Controller of its destruction.",
                DebugUtils.DebugType.Verbose);
        }

        public void NotifyDestroyedActor(NavalShip navalShip)
        {
            if (_currentActor.Equals(navalShip))
            {
                //End current turn is for the actor being destroyed
                EndTurnForCurrentActor();
            }

            //Set the pair as false, so its level should be skipped.
            var actionPair = _levelActionableActor.Find(pair => pair.One.Equals(navalShip));
            actionPair.Two = false;

            //Remove the naval ship from the list of active naval ships.
            levelNavalActors.Remove(navalShip);

            DebugUtils.DebugLogMsg($"Naval Ship: {navalShip.name} destroyed. Checking for level finish...", DebugUtils.DebugType.System);
            if (levelGoal.CheckGoalActor(navalShip))
            {
                //Game level goal was achieved
                FinishLevel(true);
            }

            if (levelGoal.CheckGameOver())
            {
                FinishLevel(false);
            }

            var actorTurnUI = actorTurnUIs.Find(turnUI => turnUI.NavalShip.Equals(navalShip));
            if (actorTurnUIs == null) return;
            actorTurnUIs.Remove(actorTurnUI);
            Destroy(actorTurnUI.gameObject);
        }

        public void NotifyDestroyedActor(NavalTarget navalTarget)
        {
            DebugUtils.DebugLogMsg($"Target: {navalTarget.name} destroyed. Checking for level finish...", DebugUtils.DebugType.System);
            levelGoal.CheckGoalActor(navalTarget);

            if (levelGoal.CheckGoalActor(navalTarget))
            {
                //Game level goal was achieved
                FinishLevel(true);
            }
        }

        private void FinishLevel(bool win)
        {
            //Prevents finishing the level more than once
            if (_finishedLevel) return;
            _finishedLevel = true;
            StopCoroutine(_levelCoroutine);

            DebugUtils.DebugLogMsg($"Level ended: {(win ? "Victory!" : "Defeat!")}", DebugUtils.DebugType.System);
            CursorController.GetSingleton().FinishLevel();
            
            endLevelPanelUI.gameObject.SetActive(true);
            endLevelPanelUI.OpenEndLevelPanel(win);
        }

        private ActorTurnUI GetActorTurnUI(NavalShip navalShip)
        {
            return actorTurnUIs.Find(actorTurnUI => actorTurnUI.NavalShip.Equals(navalShip));
        }
        
        public string GetNextLevelName() => nextLevelName;
        
        public List<NavalActor>.Enumerator GetNavalActorsEnumerator() => levelNavalActors.GetEnumerator();
    }
}