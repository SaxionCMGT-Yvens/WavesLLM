using UnityEditor;
using UnityEngine;
using UUtils.Editor;

namespace Actors.AI.LlmAI.Editor
{
    [CustomEditor(typeof(LlmScheduleSo))]
    public class LlmScheduleSoEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            var llmScheduleSo = (LlmScheduleSo) target;
            if (GUILayout.Button("Initialize Schedule"))
            {
                llmScheduleSo.Initialize();
            }
            
            if (GUILayout.Button("Rename Schedule"))
            {
                RenameScriptableObjectHelper.RenameAssetFile(llmScheduleSo, llmScheduleSo.ToString());
            }
        }
    }
}