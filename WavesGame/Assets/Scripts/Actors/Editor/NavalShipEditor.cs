using Grid.Editor;
using UnityEditor;
using UnityEngine;

namespace Actors.Editor
{
    [CustomEditor(typeof(NavalShip))]
    public class NavalShipEditor : GridActorEditor
    {
        private UnityEditor.Editor _scriptableObjectEditor;
        
        public override void OnInspectorGUI()
        {
            var myTarget = (NavalShip)target;
            //TODO change the colour
            GUILayout.Label($"Init: [{myTarget.Initiative}] - Step: [{myTarget.RemainingSteps}/{myTarget.ShipData.stats.speed}] - Acts: [{myTarget.ActionsLeft}]");
            var grid = myTarget.GetUnit();
            GUILayout.Label($"Index [{(grid == null ? -1 : grid.Index().ToString())}]", EditorStyles.boldLabel);
            
            base.OnInspectorGUI();

            var navalShipSo = myTarget.ShipData;
            if (navalShipSo == null) return;
            EditorGUILayout.Space(15);
            EditorGUILayout.HelpBox("Naval Ship SO", MessageType.Info);
            CreateCachedEditor(navalShipSo, null, ref _scriptableObjectEditor);
            _scriptableObjectEditor.OnInspectorGUI();
        }
    }
}