using Grid.Editor;
using UnityEditor;
using UnityEngine;

namespace Actors.Editor
{
    [CustomEditor(typeof(NavalShip))]
    public class NavalShipEditor : GridActorEditor
    {
        public override void OnInspectorGUI()
        {
            var myTarget = (NavalShip)target;
            GUILayout.Label($"Initiative: [{myTarget.Initiative}]");
            var grid = myTarget.GetUnit();
            GUILayout.Label($"Index [{(grid == null ? -1 : grid.Index().ToString())}]", EditorStyles.boldLabel);
            
            base.OnInspectorGUI();
        }
    }
}