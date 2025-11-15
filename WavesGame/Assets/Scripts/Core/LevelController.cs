using System;
using System.Collections;
using System.Collections.Generic;
using Actors;
using Grid;
using NaughtyAttributes;
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
            return One.CompareTo(other.One);
        }
    }

    public class LevelController : WeakSingleton<LevelController>
    {
        [Header("Data")] [SerializeField] private List<GridActor> levelActors;
        [SerializeField, ReadOnly] private List<NavalActor> levelNavalActors;
        [SerializeField, ReadOnly] private List<LevelActorPair> levelActionableActor;
        [SerializeField, ReadOnly] private List<ActorTurnUI> actorTurnUIs;

        [Header("References")] [SerializeField]
        private RectTransform actorTurnsHolder;

        [SerializeField] private ActorTurnUI actorTurnUIPrefab;

        private Coroutine _levelCoroutine;
        private NavalActor _currentActor;
        private bool _endTurn;

        private void Start()
        {
            _levelCoroutine = StartCoroutine(LevelCoroutine());
        }

        private IEnumerator LevelCoroutine()
        {
            //Wait for one frame for all elements to be initialized
            yield return null;
            //Roll initiatives and order turns
            levelActionableActor.ForEach(actorPair => actorPair.One.RollInitiative());
            levelActionableActor.Sort();
            levelActionableActor.ForEach(actorPair => AddLevelActorToTurnBar(actorPair.One));

            //Start level
            //TODO check! ships being destroyed during the iteration.

            var enumerator = levelActionableActor.GetEnumerator();
            var continueLevel = true;
            while (continueLevel)
            {
                //There are no actors left. Finish the level cycle.
                if (actorTurnUIs.Count == 0)
                {
                    continueLevel = false;
                    continue;
                }

                while(enumerator.MoveNext())
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
                        yield return new WaitUntil(() => _endTurn);
                    }
                }
                enumerator.Dispose();
                //Finished going through all characters
                //If there are no more enumerators ahead, then start from the beginning.
                enumerator = levelActionableActor.GetEnumerator();
            }

            enumerator.Dispose();

            //TODO Level ended
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
                        levelActionableActor.Add(new LevelActorPair(navalShip));
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
                //TODO current turn is for the actor being destroyed
                EndTurnForCurrentActor();
            }

            //Set the pair as false, so its level should be skipped.
            var actionPair = levelActionableActor.Find(pair => pair.One.Equals(navalShip));
            actionPair.Two = false;

            //Remove the naval ship from the list of active naval ships.
            levelNavalActors.Remove(navalShip);

            var actorTurnUI = actorTurnUIs.Find(ac => ac.NavalShip.Equals(navalShip));
            if (actorTurnUIs == null) return;
            actorTurnUIs.Remove(actorTurnUI);
            Destroy(actorTurnUI.gameObject);
        }

        private ActorTurnUI GetActorTurnUI(NavalShip navalShip)
        {
            return actorTurnUIs.Find(actorTurnUI => actorTurnUI.NavalShip.Equals(navalShip));
        }
    }
}