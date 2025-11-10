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
    public class LevelController : WeakSingleton<LevelController>
    {
        [Header("Data")] 
        [SerializeField] private List<GridActor> levelActors;
        [SerializeField, ReadOnly] private List<NavalActor> levelNavalActors;
        [SerializeField, ReadOnly] private List<NavalShip> levelActionableActor;
        [SerializeField, ReadOnly] private List<ActorTurnUI> actorTurnUIs;

        [Header("References")] 
        [SerializeField] private RectTransform actorTurnsHolder;
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
            levelActionableActor.ForEach(actor => actor.RollInitiative());
            levelActionableActor.Sort();
            levelActionableActor.ForEach(AddLevelActorToTurnBar);
            
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
                
                if (enumerator.MoveNext())
                {
                    _currentActor = enumerator.Current;
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
                        turnUI.ToggleAvailability(false);
                    }
                    else
                    {
                        yield return new WaitUntil(() => _endTurn);    
                    }
                    
                }
                else
                {
                    enumerator.Dispose();
                    //If there are no more enumerators ahead, then start from the beginning.
                    enumerator = levelActionableActor.GetEnumerator();
                }
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
                        levelActionableActor.Add(navalShip);   
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
            DebugUtils.DebugLogMsg($"Naval Actor {navalActor.name} notified Level Controller of its destruction.", DebugUtils.DebugType.Verbose);
        }

        public void NotifyDestroyedActor(NavalShip navalShip)
        {
            if (_currentActor.Equals(navalShip))
            {
                //TODO current turn is for the actor being destroyed
                EndTurnForCurrentActor();
            }
            else
            {
                levelActionableActor.Remove(navalShip);
                var actorTurnUI = actorTurnUIs.Find(ac => ac.NavalShip.Equals(navalShip));
                if (actorTurnUIs == null) return;
                actorTurnUIs.Remove(actorTurnUI);
                Destroy(actorTurnUI.gameObject);
            }
        }

        private ActorTurnUI GetActorTurnUI(NavalShip navalShip)
        {
            return actorTurnUIs.Find(actorTurnUI => actorTurnUI.NavalShip.Equals(navalShip));
        }

        // public List<NavalActor>.Enumerator GetLevelActors() => levelNavalActors.GetEnumerator();
    }
}