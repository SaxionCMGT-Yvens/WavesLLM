using Grid;
using UnityEngine;

namespace Actors
{
    public class NavalActor : GridActor
    {
        [SerializeField] private ParticleSystem damageParticles;

        [SerializeField] private NavalActorType navalType;
        public NavalActorType NavalType => navalType;

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            damageParticles.gameObject.SetActive(true);
            damageParticles.Play();
        }
    }
}