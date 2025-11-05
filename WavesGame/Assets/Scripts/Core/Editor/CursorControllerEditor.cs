using UnityEditor;
using UnityEngine;
using UUtils.Editor;

namespace Core.Editor
{
    [CustomEditor(typeof(CursorController))]
    public class CursorControllerEditor : UnityEditor.Editor
    {
        private static Color GetStateColor(CursorState state)
        {
            return state switch
            {
                CursorState.Roaming => Color.black,
                CursorState.SelectGridUnit => Color.green,
                CursorState.ShowingOptions => Color.blue,
                CursorState.Moving => Color.blueViolet,
                CursorState.OnTheMove => Color.aquamarine,
                CursorState.Targeting => Color.red,
                _ => Color.white
            };
        }

        public override void OnInspectorGUI()
        {
            var cursorController = (CursorController)target;
            var currentState = cursorController.GetState();
            var coloredStyle = new GUIStyle(GUI.skin.label)
            {
                normal =
                {
                    textColor = GetStateColor(currentState)
                }
            };
            GUILayout.Label($"Current State: {currentState.ToString()}", coloredStyle);
            GUILayout.BeginHorizontal();
            EditorDrawerHelper.DrawActiveBox("Is Active?", cursorController.IsActive());
            GUILayout.EndHorizontal();

            base.OnInspectorGUI();
        }
    }
}