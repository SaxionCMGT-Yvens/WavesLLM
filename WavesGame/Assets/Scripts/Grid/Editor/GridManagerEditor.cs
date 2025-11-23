using UnityEditor;
using UnityEngine;

namespace Grid.Editor
{
    [CustomEditor(typeof(GridManager))]
    public class GridManagerEditor : UnityEditor.Editor
    {
        private int _firstIndex;
        private int _secondIndex;
        private Vector2Int _searchIndex;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // GUILayout.Label("Search Grid by Index", EditorStyles.boldLabel);
            // _firstIndex = EditorGUILayout.IntField("First Integer", _firstIndex);
            // _secondIndex = EditorGUILayout.IntField("Second Integer", _secondIndex);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search Grid by Index", EditorStyles.boldLabel, GUILayout.Width(140));
            EditorGUILayout.LabelField("X", GUILayout.Width(12));
            _firstIndex = EditorGUILayout.IntField(_firstIndex, GUILayout.Width(40));
            EditorGUILayout.LabelField("Y", GUILayout.Width(12));
            _secondIndex = EditorGUILayout.IntField(_secondIndex, GUILayout.Width(40));
            if (GUILayout.Button("Find Grid Unit"))
            {
                SearchForMatchingObjects();
            }
            GUILayout.EndHorizontal();
        }

        private void SearchForMatchingObjects()
        {
            var gridManager = (GridManager)target;

            _searchIndex = new Vector2Int(_firstIndex, _secondIndex);
            var found = gridManager.CheckGridPosition(_searchIndex, out var unit);
            if (found)
            {
                EditorGUIUtility.PingObject(unit.gameObject);
            }
        }
    }
}