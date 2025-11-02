using UnityEngine;

namespace Grid
{
    public class GridUnit : MonoBehaviour
    {
        [SerializeField] private GridUnitType type;
        [SerializeField] private Vector2Int index;

        private void Start()
        {
            GridManager.GetSingleton().AddGridUnit(this);
        }

        public GridUnitType Type() => type;
        public void SetIndex(Vector2Int newIndex) => this.index = newIndex;
        public Vector2Int Index() => index;
    }
}