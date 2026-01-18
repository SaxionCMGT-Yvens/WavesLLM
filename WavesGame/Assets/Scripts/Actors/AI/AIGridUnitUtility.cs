/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 *
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System;
using System.Collections.Generic;
using Core;
using Grid;
using UnityEngine;
using UUtils;

namespace Actors.AI
{
    public class AIGridUnitUtility : IComparer<AIGridUnitUtility>, IComparable<AIGridUnitUtility>, IComparable
    {
        private readonly GridUnit _unit;

        public AIGridUnitUtility(GridUnit unit)
        {
            Utility = -1.0f;
            _unit = unit;
        }

        /// <summary>
        /// Calculates the utility of moving to the given unit.
        /// </summary>
        /// <param name="aiNavalShip"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static float CalculateUtilityToMoveToGridUnit(AINavalShip aiNavalShip, GridUnit unit)
        {
            var genes = aiNavalShip.GetGenesData();
            var faction = aiNavalShip.GetFaction();
            if (unit.ActorsCount() <= 0) return genes.patience;
            var actorEnumerator = unit.GetActorEnumerator();
            var utility = 0.0f;
            while (actorEnumerator.MoveNext())
            {
                var current = actorEnumerator.Current;
                if (current == null) continue;

                switch (current)
                {
                    case NavalTarget:
                        utility += genes.targetInterest;
                        break;
                    case AIBaseShip ally when ally.GetFaction().Equals(faction):
                        utility += genes.friendliness;
                        break;
                    case AIBaseShip:
                    case NavalShip:
                        //If there is an enemy there, then the AI cannot move there
                        utility = float.MinValue;
                        break;
                    case WaveActor waveActor:
                        utility -= waveActor.GetDamage();
                        break;
                }
            }

            actorEnumerator.Dispose();
            return utility;
        }

        /// <summary>
        /// Calculates the utility of moving to a unit that is directly connected to this one, but not itself.
        /// For example, if the ship is in index [2,2], this method could be used to calculate the utility of
        /// index [2,3] given the current position at [2,2].
        /// </summary>
        /// <param name="aiNavalShip"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static float CalculateProximityUtility(AINavalShip aiNavalShip, GridUnit unit)
        {
            var genes = aiNavalShip.GetGenesData();
            var faction = aiNavalShip.GetFaction();
            if (unit.ActorsCount() <= 0) return genes.patience;
            var actorEnumerator = unit.GetActorEnumerator();
            var utility = 0.0f;
            while (actorEnumerator.MoveNext())
            {
                var current = actorEnumerator.Current;
                if (current == null) continue;

                switch (current)
                {
                    case NavalTarget:
                        utility += genes.targetInterest;
                        break;
                    case AIBaseShip ally when ally.GetFaction().Equals(faction):
                        utility += genes.friendliness;
                        break;
                    case AIBaseShip enemyAI:
                        utility += AttackUtility(enemyAI, aiNavalShip, genes);
                        break;
                    case NavalShip navalShip:
                        utility += AttackUtility(navalShip, aiNavalShip, genes);
                        break;
                    case WaveActor waveActor:
                        utility -= waveActor.GetDamage();
                        break;
                }
            }

            actorEnumerator.Dispose();
            utility += CalculateActorsGridUnit(aiNavalShip, unit);
            return utility;
        }

        /// <summary>
        /// Calculates the utility of a given unit based on its distance to all the NavalActors in the environment.
        /// </summary>
        /// <param name="aiNavalShip"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        private static float CalculateActorsGridUnit(AINavalShip aiNavalShip, GridUnit unit)
        {
            var actorEnumerator = LevelController.GetSingleton().GetNavalActorsEnumerator();
            var genes = aiNavalShip.GetGenesData();
            var faction = aiNavalShip.GetFaction();
            var utility = 0.0f;
            while (actorEnumerator.MoveNext())
            {
                var current = actorEnumerator.Current;
                if (current == null) continue;
                if (current.IsMarkedForDeath()) continue;
                if (current.GetUnit() == null)
                {
                    DebugUtils.DebugLogErrorMsg($"Actor {current.GetUnit().name} has no unit.");
                    continue;
                }

                var distance = unit.DistanceTo(current.GetUnit());
                var maxDistance = genes.sight;
                var distanceFactor = 1.05f - Mathf.Min(distance, maxDistance) / maxDistance;

                switch (current)
                {
                    case NavalTarget:
                        utility += distanceFactor * genes.targetInterest;
                        break;
                    case AIBaseShip ally when ally.GetFaction().Equals(faction):
                        utility += distanceFactor * (2.0f - aiNavalShip.GetHealthRatio()) * genes.friendliness;
                        break;
                    case AIBaseShip enemyAI:
                        utility += distanceFactor * AttackUtility(enemyAI, aiNavalShip, genes);
                        break;
                    case NavalShip navalShip:
                        utility += distanceFactor * AttackUtility(navalShip, aiNavalShip, genes);
                        break;
                }
            }

            actorEnumerator.Dispose();
            return utility;
        }

        /// <summary>
        /// Calculates the utility of a possible attack at the given unit.
        /// Possible attacks at allies do not contribute to the utility.
        /// </summary>
        /// <param name="aiNavalShip"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static float CalculatePossibleAttackUtility(AINavalShip aiNavalShip, GridUnit unit)
        {
            var genes = aiNavalShip.GetGenesData();
            var faction = aiNavalShip.GetFaction();
            if (unit.ActorsCount() <= 0) return genes.patience;
            var actorEnumerator = unit.GetActorEnumerator();
            var utility = 0.0f;
            while (actorEnumerator.MoveNext())
            {
                var current = actorEnumerator.Current;
                if (current == null) continue;

                switch (current)
                {
                    case NavalTarget:
                        utility += genes.targetInterest;
                        break;
                    case AIBaseShip ally when ally.GetFaction().Equals(faction):
                        //Possibly aiming at an ally has no impact in the utility
                        break;
                    case AIBaseShip enemyAI:
                        utility += AttackUtility(enemyAI, aiNavalShip, genes);
                        break;
                    case NavalShip navalShip:
                        utility += AttackUtility(navalShip, aiNavalShip, genes);
                        break;
                    case WaveActor waveActor:
                        utility += CalculateAttackAtWaveUtility(aiNavalShip, waveActor);
                        break;
                }
            }

            actorEnumerator.Dispose();
            return utility;
        }

        /// <summary>
        /// Calculates the utility of attacking at the given unit.
        /// Possible attacks at allies contribute with float.MinValue as utility to prevent friendly fire.
        /// </summary>
        /// <param name="aiNavalShip"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static float CalculateAttackUtility(AINavalShip aiNavalShip, GridUnit unit)
        {
            var genes = aiNavalShip.GetGenesData();
            var faction = aiNavalShip.GetFaction();
            if (unit.ActorsCount() <= 0) return float.MinValue;
            var actorEnumerator = unit.GetActorEnumerator();
            var utility = 0.0f;
            while (actorEnumerator.MoveNext())
            {
                var current = actorEnumerator.Current;
                if (current == null) continue;

                switch (current)
                {
                    case NavalTarget:
                        utility += genes.targetInterest;
                        break;
                    case AIBaseShip ally when ally.GetFaction().Equals(faction):
                        utility = float.MinValue;
                        break;
                    case AIBaseShip enemyAI:
                        utility += AttackUtility(enemyAI, aiNavalShip, genes);
                        break;
                    case NavalShip navalShip:
                        utility += AttackUtility(navalShip, aiNavalShip, genes);
                        break;
                    case WaveActor waveActor:
                    {
                        utility += CalculateAttackAtWaveUtility(aiNavalShip, waveActor);
                    }
                        break;
                }
            }

            actorEnumerator.Dispose();
            return utility;
        }

        /// <summary>
        /// Calculates the utility of attacking a given target considering the AI genes and health factor.
        /// The formula is [aggressiveness + (selfRatio - targetRatio) * selfPreservation]
        /// Aggressiveness and SelfPreservation are the values from the AI genes.
        /// SelfRatio and TargetRatio are the ratio [0,1] of the respective health of the AI and the target.
        /// </summary>
        /// <param name="targetActor"></param>
        /// <param name="selfActor"></param>
        /// <param name="genes"></param>
        /// <returns></returns>
        private static float AttackUtility(NavalActor targetActor, AINavalShip selfActor, AIGenesSO genes)
        {
            var targetHealthRatio = targetActor.GetHealthRatio();
            var selfHealthRatio = selfActor.GetHealthRatio();
            return genes.aggressiveness + (selfHealthRatio - targetHealthRatio) * genes.selfPreservation;
        }

        /// <summary>
        /// Calculates the utility of attacking a given wave considering its area of effect. 
        /// </summary>
        /// <param name="aiNavalShip"></param>
        /// <param name="waveActor"></param>
        /// <returns></returns>
        private static float CalculateAttackAtWaveUtility(AINavalShip aiNavalShip, WaveActor waveActor)
        {
            var affectedByWave = waveActor.GetUnitsAffectedByWaveAttack();
            var waveUtility = 0.0f;
            var enemiesHitByWave = 0;
            affectedByWave.ForEach(waveUnit =>
            {
                if (waveUnit.ActorsCount() <= 0) return;
                var waveEnumerator = waveUnit.GetActorEnumerator();
                while (waveEnumerator.MoveNext())
                {
                    var waveCurrent = waveEnumerator.Current;
                    if (waveCurrent == null) continue;
                    waveUtility += CalculateActorInWaveRangeUtility(waveActor, aiNavalShip, waveCurrent,
                        out var hitEnemy);
                    if (hitEnemy) enemiesHitByWave++;
                }

                waveEnumerator.Dispose();
            });
            //Reset the wave utility if no enemy at all will be hit by it.
            if (enemiesHitByWave <= 0)
            {
                waveUtility = float.MinValue;
            }

            return waveUtility;
        }

        /// <summary>
        /// Calculates the utility of attacking a given actor by a wave area of effect attack.
        /// Used by the [CalculateAttackAtWaveUtility] method while iterating over all the actors affected by the wave.
        /// </summary>
        /// <param name="waveActor"></param>
        /// <param name="aiNavalShip"></param>
        /// <param name="actor"></param>
        /// <param name="hitEnemy"></param>
        /// <returns></returns>
        private static float CalculateActorInWaveRangeUtility(WaveActor waveActor, AINavalShip aiNavalShip,
            GridActor actor,
            out bool hitEnemy)
        {
            hitEnemy = false;
            var actorInWaveRangeUtility = 0.0f;
            var genes = aiNavalShip.GetGenesData();
            if (actor.Equals(aiNavalShip)) return -genes.selfPreservation; //Negative utility if attacks itself

            var faction = aiNavalShip.GetFaction();
            if (actor.Equals(waveActor)) return 0; //No utility for self-wave-attack + prevent infinite recursion

            hitEnemy = true;
            switch (actor)
            {
                case NavalTarget:
                    //Utility is equal the likeliness of attacking a target
                    actorInWaveRangeUtility += genes.targetInterest;
                    break;
                case AIBaseShip ally when ally.GetFaction().Equals(faction):
                    //Generates negative utility for hitting an ally
                    actorInWaveRangeUtility -= genes.friendliness;
                    //Negate the hit enemy flag so to prevent waves that only hit allies from having a positive utility
                    hitEnemy = false;
                    break;
                case AIBaseShip enemyAI:
                    //2.0f instead of 1.0f, so enemies with full health (1.0f) still generate positive utility
                    actorInWaveRangeUtility = genes.aggressiveness * (2.0f - enemyAI.GetHealthRatio());
                    break;
                case NavalShip navalShip:
                    //Same as enemyAI
                    actorInWaveRangeUtility = genes.aggressiveness * (2.0f - navalShip.GetHealthRatio());
                    break;
                case WaveActor anotherWaveActor:
                    //Uses the damage of the wave as utility.
                    //TODO allow recursive wave navigation to assess chain-reaction wave effects
                    actorInWaveRangeUtility = anotherWaveActor.GetDamage();
                    break;
            }

            return actorInWaveRangeUtility;
        }

        public float Utility { get; set; }

        public GridUnit GetUnit() => _unit;

        public int Compare(AIGridUnitUtility x, AIGridUnitUtility y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (y is null) return 1;
            if (x is null) return -1;
            return y.Utility.CompareTo(x.Utility);
        }

        public int CompareTo(AIGridUnitUtility other)
        {
            if (ReferenceEquals(this, other)) return 0;
            return other is null ? 1 : other.Utility.CompareTo(Utility);
        }

        public int CompareTo(object other)
        {
            if (ReferenceEquals(this, other)) return 0;
            return other switch
            {
                null => 1,
                AIGridUnitUtility otherAI => CompareTo(otherAI),
                _ => -1
            };
        }

        public override string ToString()
        {
            return $"{_unit.Index()} - U = {Utility}";
        }
    }
}