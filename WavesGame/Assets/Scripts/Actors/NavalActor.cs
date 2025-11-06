using System.Collections;
using Grid;
using UI;
using UnityEngine;
using UUtils;

namespace Actors
{
    public class NavalActor : GridActor
    {
        [SerializeField] private ParticleSystem damageParticles;
        [SerializeField] private ParticleSystem destroyParticles;
        [SerializeField] private NavalActorType navalType;
        [SerializeField] private FillBar healthBar;

        private void Awake()
        {
            AssessUtils.CheckRequirement(ref healthBar, this);
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            var ratio = GetHealthRatio();
            healthBar.SetFillFactor(ratio, 1 - ratio);
            damageParticles.gameObject.SetActive(true);
            damageParticles.Play();
        }

        protected override void DestroyActor()
        {
            StartCoroutine(DestroyCoroutine());
        }

        private IEnumerator DestroyCoroutine()
        {
            var particles = Instantiate(destroyParticles, transform.position, Quaternion.identity);
            particles.Play();
            var totalTime = particles.main.duration;
            DebugUtils.DebugLogMsg($"Naval {name} being destroyed in {totalTime}.", DebugUtils.DebugType.Temporary);
            currentUnit.RemoveActor(this);
            yield return new WaitForSeconds(totalTime);
            DebugUtils.DebugLogMsg($"Timer is up for {name}!", DebugUtils.DebugType.Temporary);
            Destroy(gameObject);
        }
        
        public NavalActorType NavalType => navalType;
    }
}