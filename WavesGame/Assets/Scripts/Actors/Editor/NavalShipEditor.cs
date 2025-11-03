using UnityEditor;
using UnityEngine;

namespace Actors.Editor
{
    [CustomEditor(typeof(NavalShip))]
    public class NavalShipEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var myTarget = (NavalShip)target;
            var grid = myTarget.GetUnit();
            GUILayout.Label($"Index [{(grid == null ? -1 : grid.Index().ToString())}]", EditorStyles.boldLabel);
            base.OnInspectorGUI();
        }
    }
}