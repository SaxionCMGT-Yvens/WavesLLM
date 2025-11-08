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
            levelActionableActor.ForEach(AddLevelToTurnBar);
            
            //Start level
            foreach (var turnUI in actorTurnUIs)
            {
                _endTurn = false;
                turnUI.ToggleAvailability(true);
                //TODO allow actor to act
                _currentActor = turnUI.NavalShip;
                yield return new WaitUntil(() => _endTurn);
                turnUI.ToggleAvailability(false);
            }
        }

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

        private void AddLevelToTurnBar(NavalShip navalShip)
        {
            var newActorTurnUI = Instantiate(actorTurnUIPrefab, actorTurnsHolder);
            newActorTurnUI.Initialize(navalShip);
            actorTurnUIs.Add(newActorTurnUI);
        }

        public bool IsCurrentActor(NavalActor navalActor)
        {
            return _currentActor.Equals(navalActor);
        }

        // public List<NavalActor>.Enumerator GetLevelActors() => levelNavalActors.GetEnumerator();
    }
}