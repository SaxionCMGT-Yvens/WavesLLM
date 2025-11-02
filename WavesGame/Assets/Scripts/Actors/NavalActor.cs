using Grid;
using UnityEngine;

namespace Actors
{
    public class NavalActor : GridActor
    {
        [SerializeField] private NavalActorType type;
        public NavalActorType Type => type;
    }
}