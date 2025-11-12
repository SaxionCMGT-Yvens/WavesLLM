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
            //TODO change the colour
            GUILayout.Label($"Init: [{myTarget.Initiative}] - Step: [{myTarget.RemainingSteps}/{myTarget.ShipData.stats.speed}] - Acts: [{myTarget.ActionsLeft}]");
            var grid = myTarget.GetUnit();
            GUILayout.Label($"Index [{(grid == null ? -1 : grid.Index().ToString())}]", EditorStyles.boldLabel);
            
            base.OnInspectorGUI();
        }
    }
}