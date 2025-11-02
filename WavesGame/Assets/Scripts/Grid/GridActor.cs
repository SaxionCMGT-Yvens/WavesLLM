using UnityEngine;

namespace Grid
{
    public class GridActor : MonoBehaviour
    {
        [SerializeField] private bool blockGridUnit;

        public bool BlockGridUnit => blockGridUnit;
    }
}