/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 *
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using UnityEditor;

namespace Core.Editor
{
    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : UnityEditor.Editor
    {
        private UnityEditor.Editor _scriptableObjectEditor;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var myBehaviour = (GameManager)target;
            var settings = myBehaviour.GetSettings();

            if (settings == null) return;
            EditorGUILayout.Space(15);
            EditorGUILayout.HelpBox("Settings", MessageType.Info);
            CreateCachedEditor(settings, null, ref _scriptableObjectEditor);
            _scriptableObjectEditor.OnInspectorGUI();
        }
    }
}