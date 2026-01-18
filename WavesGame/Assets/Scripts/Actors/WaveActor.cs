/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 *
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System;
using System.Collections.Generic;
using Grid;
using NaughtyAttributes;
using UnityEngine;
using UUtils;

namespace Actors
{
    #region WaveDirectionSprite

    [Serializable]
    public class WaveDirectionSprite : Pair<GridMoveType, Sprite>
    {
        public WaveDirectionSprite(GridMoveType one, Sprite two) : base(one, two)
        {
        }
    }

    #endregion

    public class WaveActor : GridActor
    {
        [SerializeField] private GridMoveType waveDirection;
        [SerializeField] private SpriteRenderer waveDirectionSpriteRenderer;
        [SerializeField] private List<WaveDirectionSprite> waveDirectionSprites;
        [SerializeField] private ParticleSystem damageParticles;
        [SerializeField] private int areaOfEffect;
        [SerializeField] private int stepAreaDistance;
        [SerializeField] private int waveDamage;

        protected override void Start()
        {
            base.Start();
            UpdateWaveDirectionSprite();
        }

        [Button("Update Wave Direction Sprite")]
        private void UpdateWaveDirectionSprite()
        {
            var waveDirectionSprite = waveDirectionSprites.Find(pair => pair.One.Equals(waveDirection));
            waveDirectionSpriteRenderer.sprite = waveDirectionSprite.Two;
        }

        public override bool TakeDamage(int damage)
        {
            DebugUtils.DebugLogMsg($"Wave actor {name} was attacked with {damage}.", DebugUtils.DebugType.Verbose);
            var destroyed = base.TakeDamage(damage);
            if (damage <= 0) return false;
            if (damageParticles != null)
            {
                damageParticles.Play();
            }

            var attackArea = GridManager.GetSingleton()
                .GetGridUnitsForMoveType(waveDirection, GetUnit().Index(), areaOfEffect);
            attackArea.ForEach(unit =>
            {
                if (unit.ActorsCount() > 0)
                {
                    unit.DamageActors(damage);
                }
                else
                {
                    var unitTransform = unit.transform;
                    var particles = Instantiate(damageParticles, unitTransform.position, unitTransform.rotation);
                    particles.Play();
                    var totalTime = particles.main.duration;
                    Destroy(particles.gameObject, totalTime);
                }
            });
            return destroyed;
        }

        public List<GridUnit> GetUnitsAffectedByWaveAttack()
        {
            return GridManager.GetSingleton().GetGridUnitsForMoveType(waveDirection, GetUnit().Index(), areaOfEffect);
        }

        public override GridStepEffectResult StepEffect(GridActor stepper)
        {
            var pushArea = GridManager.GetSingleton()
                .GetGridUnitsForMoveType(waveDirection, GetUnit().Index(), stepAreaDistance);
            //From the places to push the actor, get all of them that are not blocked (i.e., that the actor can be move to).
            var pushUnblockedArea = pushArea.FindAll(unit => unit.Type() != GridUnitType.Blocked);

            //If there are no valid places to push, ignore the step effect, cause damage and block movement from proceeding.
            if (pushUnblockedArea.Count == 0) return new GridStepEffectResult(false, null, true, waveDamage);

            //Gets a random position from the list of pushable spaces.
            var pushTo = RandomHelper<GridUnit>.GetRandomFromList(pushUnblockedArea);
            return new GridStepEffectResult(false, pushTo, true, waveDamage);
        }

        public float GetDamage() => waveDamage;
        
        public override string ToString()
        {
            return $" {base.ToString()}; waveDirection={waveDirection}; areaOfEffect={areaOfEffect}; stepAreaDistance={stepAreaDistance}; waveDamage={waveDamage} ";
        }
    }
}