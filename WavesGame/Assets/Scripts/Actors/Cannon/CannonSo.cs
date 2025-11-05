using Grid;
using UnityEngine;

namespace Actors.Cannon
{
    [CreateAssetMenu(fileName = "New Cannon", menuName = "Waves/Cannon", order = 1)]
    public class CannonSo : ScriptableObject
    {
        public int area;
        public int deadZone;
        public int damage;
        public GridMoveType targetAreaType;
    }
}