/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 * 
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System.Collections;
using Core;
using Grid;
using UI;
using UnityEngine;
using UUtils;

namespace Actors
{
    public class NavalActor : GridActor
    {
        [SerializeField] private ParticleSystem damageParticles;
        [SerializeField] private ParticleSystem missParticles;
        [SerializeField] private ParticleSystem destroyParticles;
        [SerializeField] private NavalActorType navalType;
        [SerializeField] private FillBar healthBar;

        protected virtual void Awake()
        {
            AssessUtils.CheckRequirement(ref healthBar, this);
        }

        protected override void Start()
        {
            base.Start();
            LevelController.GetSingleton().AddLevelActor(this);
        }

        public override void TakeDamage(int damage)
        {
            //TODO replace MaxValue with some more controlled value
            damage = Mathf.Clamp(damage, 0, int.MaxValue);
            if (damage > 0)
            {
                base.TakeDamage(damage);
                var ratio = GetHealthRatio();
                healthBar.SetFillFactor(ratio, 1 - ratio);
                damageParticles.gameObject.SetActive(true);
                damageParticles.Play();
            }
            else
            {
                missParticles.gameObject.SetActive(true);
                missParticles.Play();
            }
        }

        protected override void DestroyActor()
        {
            StartCoroutine(DestroyCoroutine());
            NotifyLevelController();
        }

        protected virtual void NotifyLevelController()
        {
            LevelController.GetSingleton().NotifyDestroyedActor(this);
        }

        private IEnumerator DestroyCoroutine()
        {
            //Give one extra frame to wait for the damage particles to play before executing the death particles.
            yield return new WaitForEndOfFrame();
            DebugUtils.DebugLogMsg(
                $"Destroy {name} is waiting for particles to destroy itself... [{!damageParticles.gameObject.activeInHierarchy}, {damageParticles.isStopped}]",
                DebugUtils.DebugType.Verbose);
            yield return new WaitUntil(() =>
                !damageParticles.gameObject.activeInHierarchy && damageParticles.isStopped);
            var particles = Instantiate(destroyParticles, transform.position, Quaternion.identity);
            particles.Play();
            var totalTime = particles.main.duration;
            DebugUtils.DebugLogMsg($"Naval {name} being destroyed in {totalTime}.", DebugUtils.DebugType.Verbose);
            currentUnit.RemoveActor(this);
            yield return new WaitForSeconds(totalTime);
            DebugUtils.DebugLogMsg($"Destroy {name} game object!", DebugUtils.DebugType.Verbose);
            Destroy(gameObject);
        }

        public NavalActorType NavalType => navalType;
    }
}